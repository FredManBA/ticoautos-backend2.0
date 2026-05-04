using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TicoAutos.Domain.Constants;
using TicoAutos.Domain.Entities;
using TicoAutos.Domain.Interfaces;

namespace TicoAutos.Infrastructure.Identity;

/// <summary>
/// Handles JWT token generation, user registration and login logic.
/// </summary>
public class IdentityService : IIdentityService
{
    private const string EmailAlreadyRegisteredMessage = "El correo ya está registrado.";
    private const string CedulaAlreadyRegisteredMessage = "La cédula ya está registrada.";
    private const string GoogleRegistrationPurpose = "google_registration";
    private const string TwoFactorPurpose = "two_factor";
    private const string VerificationEmailFailureMessage =
        "La cuenta fue creada, pero no fue posible enviar el correo de verificacion. Intente reenviarlo mas tarde.";

    private readonly IConfiguration _config;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICedulaValidationService _cedulaValidationService;
    private readonly IEmailSender _emailSender;
    private readonly ITwoFactorSender _twoFactorSender;

    public IdentityService(
        IConfiguration config,
        IUnitOfWork unitOfWork,
        ICedulaValidationService cedulaValidationService,
        IEmailSender emailSender,
        ITwoFactorSender twoFactorSender)
    {
        _config = config;
        _unitOfWork = unitOfWork;
        _cedulaValidationService = cedulaValidationService;
        _emailSender = emailSender;
        _twoFactorSender = twoFactorSender;
    }

    /// <summary>
    /// Generates a JWT token for an authenticated user.
    /// </summary>
    public string GenerateToken(string email, string userId)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? ""));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("id", userId)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Registers a new pending user after validating cedula and sends the verification email.
    /// </summary>
    public async Task<(bool Success, int UserId, string FullName, string Error)> RegisterAsync(
        string email, string password, string cedula, string phoneNumber)
    {
        if (await _unitOfWork.Users.ExistsAsync(email))
            return (false, 0, string.Empty, EmailAlreadyRegisteredMessage);

        if (await _unitOfWork.Users.ExistsByCedulaAsync(cedula))
            return (false, 0, string.Empty, CedulaAlreadyRegisteredMessage);

        var cedulaValidation = await _cedulaValidationService.ValidateAsync(cedula);
        if (!cedulaValidation.IsValid)
            return (false, 0, string.Empty, cedulaValidation.Error ?? "No fue posible validar la cedula en este momento.");

        var user = new User
        {
            Email = email,
            Name = cedulaValidation.FullName,
            Cedula = cedulaValidation.Cedula,
            PhoneNumber = phoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            AccountStatus = AccountStatuses.Pending,
            IsEmailVerified = false
        };

        SetVerificationToken(user);

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var emailSent = await TrySendVerificationEmailAsync(user);
        if (!emailSent)
            return (false, user.Id, user.Name, VerificationEmailFailureMessage);

        return (true, user.Id, user.Name, string.Empty);
    }

    /// <summary>
    /// Validates user credentials and returns a JWT token if the account is active.
    /// </summary>
    public async Task<(bool Success, bool RequiresTwoFactor, string Token, string TwoFactorToken, string Email, string FullName, string Error)> LoginAsync(
        string email, string password)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        if (user is null)
            return (false, false, string.Empty, string.Empty, string.Empty, string.Empty, "Invalid credentials.");

        if (user.AuthProvider != AuthProviders.Local)
            return (false, false, string.Empty, string.Empty, string.Empty, string.Empty, "Use Google sign-in for this account.");

        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, false, string.Empty, string.Empty, string.Empty, string.Empty, "Invalid credentials.");

        if (!user.IsEmailVerified || user.AccountStatus == AccountStatuses.Pending)
            return (false, false, string.Empty, string.Empty, string.Empty, string.Empty, "Account pending email verification.");

        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            return (false, false, string.Empty, string.Empty, string.Empty, string.Empty, "Phone number is required for two-factor authentication.");

        var codeSent = await TrySendTwoFactorCodeAsync(user.PhoneNumber);
        if (!codeSent)
            return (false, false, string.Empty, string.Empty, string.Empty, string.Empty, "No fue posible enviar el codigo de verificacion.");

        var twoFactorToken = GenerateTwoFactorToken(user);
        return (true, true, string.Empty, twoFactorToken, user.Email, user.Name, string.Empty);
    }

    public async Task<(bool Success, string Token, string Email, string FullName, string Error)> VerifyTwoFactorAsync(
        string twoFactorToken,
        string code)
    {
        var (isValid, userId, phoneNumber) = ValidateTwoFactorToken(twoFactorToken);
        if (!isValid)
            return (false, string.Empty, string.Empty, string.Empty, "Invalid or expired two-factor token.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user is null || user.AuthProvider != AuthProviders.Local)
            return (false, string.Empty, string.Empty, string.Empty, "Invalid two-factor token.");

        if (user.PhoneNumber != phoneNumber)
            return (false, string.Empty, string.Empty, string.Empty, "Invalid two-factor token.");

        var codeIsValid = await TryCheckTwoFactorCodeAsync(phoneNumber, code);
        if (!codeIsValid)
            return (false, string.Empty, string.Empty, string.Empty, "Invalid verification code.");

        var token = GenerateToken(user.Email, user.Id.ToString());
        return (true, token, user.Email, user.Name, string.Empty);
    }

    /// <summary>
    /// Activates a pending account when the verification token is valid.
    /// </summary>
    public async Task<(bool Success, string Error)> VerifyEmailAsync(string token)
    {
        var user = await _unitOfWork.Users.GetByEmailVerificationTokenAsync(token);

        if (user is null)
            return (false, "Invalid verification token.");

        if (user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            return (false, "Verification token expired.");

        user.IsEmailVerified = true;
        user.AccountStatus = AccountStatuses.Active;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAt = null;

        await _unitOfWork.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Message, string Error)> ResendVerificationAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        if (user is null)
            return (true, "Si la cuenta existe y esta pendiente, se enviara un nuevo correo de verificacion.", string.Empty);

        if (user.IsEmailVerified || user.AccountStatus == AccountStatuses.Active)
            return (false, string.Empty, "La cuenta ya esta verificada.");

        SetVerificationToken(user);
        await _unitOfWork.SaveChangesAsync();

        var emailSent = await TrySendVerificationEmailAsync(user);
        if (!emailSent)
            return (false, string.Empty, "No fue posible reenviar el correo de verificacion en este momento.");

        return (true, "Correo de verificacion reenviado.", string.Empty);
    }

    public async Task<(bool Success, bool RequiresCedula, string Token, string RegistrationToken, string Email, string FullName, string Error)> GoogleSignInAsync(
        string email,
        string fullName,
        string googleId)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(googleId))
            return (false, false, string.Empty, string.Empty, string.Empty, string.Empty, "Google account information is incomplete.");

        var googleUser = await _unitOfWork.Users.GetByExternalLoginAsync(AuthProviders.Google, googleId);
        if (googleUser is not null)
        {
            var token = GenerateToken(googleUser.Email, googleUser.Id.ToString());
            return (true, false, token, string.Empty, googleUser.Email, googleUser.Name, string.Empty);
        }

        var userWithEmail = await _unitOfWork.Users.GetByEmailAsync(email);
        if (userWithEmail is not null)
            return (false, false, string.Empty, string.Empty, email, fullName, "Email already registered with another sign-in method.");

        var registrationToken = GenerateGoogleRegistrationToken(email, fullName, googleId);
        return (true, true, string.Empty, registrationToken, email, fullName, string.Empty);
    }

    public async Task<(bool Success, string Token, string Email, string FullName, string Error)> CompleteGoogleRegistrationAsync(
        string registrationToken,
        string cedula)
    {
        var (isValid, email, fullName, googleId) = ValidateGoogleRegistrationToken(registrationToken);
        if (!isValid)
            return (false, string.Empty, string.Empty, string.Empty, "Invalid or expired Google registration token.");

        if (await _unitOfWork.Users.ExistsAsync(email))
            return (false, string.Empty, email, fullName, EmailAlreadyRegisteredMessage);

        if (await _unitOfWork.Users.ExistsByCedulaAsync(cedula))
            return (false, string.Empty, email, fullName, CedulaAlreadyRegisteredMessage);

        var cedulaValidation = await _cedulaValidationService.ValidateAsync(cedula);
        if (!cedulaValidation.IsValid)
            return (false, string.Empty, email, fullName, cedulaValidation.Error ?? "No fue posible validar la cedula en este momento.");

        var user = new User
        {
            Email = email,
            Name = cedulaValidation.FullName,
            Cedula = cedulaValidation.Cedula,
            PasswordHash = string.Empty,
            AuthProvider = AuthProviders.Google,
            ExternalProviderId = googleId,
            AccountStatus = AccountStatuses.Active,
            IsEmailVerified = true
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var token = GenerateToken(user.Email, user.Id.ToString());
        return (true, token, user.Email, user.Name, string.Empty);
    }

    private async Task<bool> TrySendVerificationEmailAsync(User user)
    {
        try
        {
            var verificationLink = BuildVerificationLink(user.EmailVerificationToken!);
            await _emailSender.SendVerificationEmailAsync(user.Email, user.Name, verificationLink);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    private async Task<bool> TrySendTwoFactorCodeAsync(string phoneNumber)
    {
        try
        {
            return await _twoFactorSender.SendCodeAsync(phoneNumber);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    private async Task<bool> TryCheckTwoFactorCodeAsync(string phoneNumber, string code)
    {
        try
        {
            return await _twoFactorSender.CheckCodeAsync(phoneNumber, code);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    private void SetVerificationToken(User user)
    {
        user.EmailVerificationToken = GenerateVerificationToken();
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
    }

    private string BuildVerificationLink(string token)
    {
        var verifyUrl = _config["EmailVerification:VerifyUrl"]
            ?? throw new InvalidOperationException("EmailVerification:VerifyUrl is missing.");

        return $"{verifyUrl}?token={Uri.EscapeDataString(token)}";
    }

    private string GenerateGoogleRegistrationToken(string email, string fullName, string googleId)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? ""));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Name, fullName),
            new Claim("google_id", googleId),
            new Claim("purpose", GoogleRegistrationPurpose)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateTwoFactorToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? ""));

        var claims = new[]
        {
            new Claim("id", user.Id.ToString()),
            new Claim("phone_number", user.PhoneNumber!),
            new Claim("purpose", TwoFactorPurpose)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private (bool IsValid, string Email, string FullName, string GoogleId) ValidateGoogleRegistrationToken(string token)
    {
        try
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? ""));

            var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(1)
            }, out _);

            var purpose = principal.FindFirst("purpose")?.Value;
            var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var fullName = principal.FindFirst(ClaimTypes.Name)?.Value;
            var googleId = principal.FindFirst("google_id")?.Value;

            if (purpose != GoogleRegistrationPurpose ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(googleId))
                return (false, string.Empty, string.Empty, string.Empty);

            return (true, email, fullName ?? email, googleId);
        }
        catch (ArgumentException)
        {
            return (false, string.Empty, string.Empty, string.Empty);
        }
        catch (SecurityTokenException)
        {
            return (false, string.Empty, string.Empty, string.Empty);
        }
    }

    private (bool IsValid, int UserId, string PhoneNumber) ValidateTwoFactorToken(string token)
    {
        try
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? ""));

            var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.FromMinutes(1)
            }, out _);

            var purpose = principal.FindFirst("purpose")?.Value;
            var userIdValue = principal.FindFirst("id")?.Value;
            var phoneNumber = principal.FindFirst("phone_number")?.Value;

            if (purpose != TwoFactorPurpose ||
                !int.TryParse(userIdValue, out var userId) ||
                string.IsNullOrWhiteSpace(phoneNumber))
                return (false, 0, string.Empty);

            return (true, userId, phoneNumber);
        }
        catch (ArgumentException)
        {
            return (false, 0, string.Empty);
        }
        catch (SecurityTokenException)
        {
            return (false, 0, string.Empty);
        }
    }

    private static string GenerateVerificationToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .Replace("=", string.Empty, StringComparison.Ordinal);
    }
}
