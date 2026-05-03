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
    private const string VerificationEmailFailureMessage =
        "La cuenta fue creada, pero no fue posible enviar el correo de verificacion. Intente reenviarlo mas tarde.";

    private readonly IConfiguration _config;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICedulaValidationService _cedulaValidationService;
    private readonly IEmailSender _emailSender;

    public IdentityService(
        IConfiguration config,
        IUnitOfWork unitOfWork,
        ICedulaValidationService cedulaValidationService,
        IEmailSender emailSender)
    {
        _config = config;
        _unitOfWork = unitOfWork;
        _cedulaValidationService = cedulaValidationService;
        _emailSender = emailSender;
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
        string email, string password, string cedula)
    {
        if (await _unitOfWork.Users.ExistsAsync(email))
            return (false, 0, string.Empty, "Email already registered.");

        if (await _unitOfWork.Users.ExistsByCedulaAsync(cedula))
            return (false, 0, string.Empty, "Cedula already registered.");

        var cedulaValidation = await _cedulaValidationService.ValidateAsync(cedula);
        if (!cedulaValidation.IsValid)
            return (false, 0, string.Empty, cedulaValidation.Error ?? "Cedula validation failed.");

        var user = new User
        {
            Email = email,
            Name = cedulaValidation.FullName,
            Cedula = cedulaValidation.Cedula,
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
    public async Task<(bool Success, string Token, string Error)> LoginAsync(
        string email, string password)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, string.Empty, "Invalid credentials.");

        if (!user.IsEmailVerified || user.AccountStatus == AccountStatuses.Pending)
            return (false, string.Empty, "Account pending email verification.");

        var token = GenerateToken(user.Email, user.Id.ToString());
        return (true, token, string.Empty);
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

    private static string GenerateVerificationToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .Replace("=", string.Empty, StringComparison.Ordinal);
    }
}
