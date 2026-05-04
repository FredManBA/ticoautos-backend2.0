// TicoAutos.WebApi/Controllers/AuthController.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TicoAutos.Application.DTOs;
using TicoAutos.Domain.Constants;
using TicoAutos.Domain.Interfaces;
using TicoAutos.WebApi.Auth;

namespace TicoAutos.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly IConfiguration _config;

    public AuthController(IIdentityService identityService, IConfiguration config)
    {
        _identityService = identityService;
        _config = config;
    }

    /// <summary>
    /// Authenticates a verified user and returns a JWT token.
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, requiresTwoFactor, token, twoFactorToken, email, fullName, error) =
            await _identityService.LoginAsync(request.Email, request.Password);

        if (!success)
            return Unauthorized(new { message = error });

        return Ok(new LoginResponse(
            requiresTwoFactor,
            string.IsNullOrWhiteSpace(token) ? null : token,
            string.IsNullOrWhiteSpace(twoFactorToken) ? null : twoFactorToken,
            email,
            fullName,
            requiresTwoFactor ? "Código de verificación enviado." : "Login correcto."));
    }

    [HttpPost("verify-2fa")]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
    {
        var (success, token, email, fullName, error) =
            await _identityService.VerifyTwoFactorAsync(request.TemporaryToken, request.Code);

        if (!success)
            return Unauthorized(new { message = error });

        return Ok(new AuthResponse(token, email, fullName));
    }

    /// <summary>
    /// Registers a new pending user and sends an email verification link.
    /// POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, userId, fullName, error) = await _identityService.RegisterAsync(
            request.Email, request.Password, request.Cedula, request.PhoneNumber);

        if (!success)
        {
            if (userId > 0)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = error });

            if (IsCedulaProviderUnavailable(error))
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = error });

            if (IsDuplicateConflict(error))
                return Conflict(new { message = error });

            return BadRequest(new { message = error });
        }

        return Ok(new RegistrationResponse(
            userId,
            fullName,
            request.Email,
            AccountStatuses.Pending,
            "Usuario registrado. Revise su correo para activar la cuenta."));
    }

    /// <summary>
    /// Validates a cedula against the external electoral registry and returns the holder name.
    /// GET /api/auth/cedula/{cedula}
    /// </summary>
    [HttpGet("cedula/{cedula}")]
    [AllowAnonymous]
    public async Task<IActionResult> LookupCedula(
        [FromRoute] string cedula,
        [FromServices] ICedulaValidationService cedulaValidationService,
        [FromServices] IUnitOfWork unitOfWork)
    {
        if (string.IsNullOrWhiteSpace(cedula) || !Regex.IsMatch(cedula, @"^\d{9}$"))
            return BadRequest(new { message = "La cédula debe tener 9 dígitos, sin guiones ni espacios." });

        if (await unitOfWork.Users.ExistsByCedulaAsync(cedula))
            return Conflict(new { message = "La cédula ya está registrada." });

        var validation = await cedulaValidationService.ValidateAsync(cedula);
        if (!validation.IsValid)
        {
            var message = validation.Error ?? "No fue posible validar la cedula en este momento.";
            if (IsCedulaProviderUnavailable(message))
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message });

            if (IsCedulaNotFound(message))
                return NotFound(new { message });

            return BadRequest(new { message });
        }

        return Ok(new CedulaLookupResponse(validation.Cedula, validation.FullName));
    }

    /// <summary>
    /// Activates a pending user account with the email verification token.
    /// GET /api/auth/verify-email?token=...
    /// </summary>
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { message = "Token de verificacion requerido." });

        var (success, error) = await _identityService.VerifyEmailAsync(token);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message = "Cuenta verificada correctamente." });
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Correo requerido." });

        var (success, message, error) = await _identityService.ResendVerificationAsync(request.Email);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new { message });
    }

    [HttpGet("google")]
    public IActionResult GoogleSignIn()
    {
        if (!IsGoogleAuthConfigured())
            return Redirect(BuildGoogleFrontendRedirect("error", new Dictionary<string, string?>
            {
                ["message"] = "Google authentication is not configured."
            }));

        var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth");
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync(ExternalAuthSchemes.GoogleExternalCookie);
        if (!result.Succeeded || result.Principal is null)
            return Redirect(BuildGoogleFrontendRedirect("error", new Dictionary<string, string?>
            {
                ["message"] = "Google authentication failed."
            }));

        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var fullName = result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? email;
        var googleId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await HttpContext.SignOutAsync(ExternalAuthSchemes.GoogleExternalCookie);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(googleId))
            return Redirect(BuildGoogleFrontendRedirect("error", new Dictionary<string, string?>
            {
                ["message"] = "Google account did not provide the required profile data."
            }));

        var googleSignIn = await _identityService.GoogleSignInAsync(email, fullName ?? email, googleId);

        if (!googleSignIn.Success)
            return Redirect(BuildGoogleFrontendRedirect("error", new Dictionary<string, string?>
            {
                ["message"] = googleSignIn.Error
            }));

        if (googleSignIn.RequiresCedula)
            return Redirect(BuildGoogleFrontendRedirect("requires_cedula", new Dictionary<string, string?>
            {
                ["registrationToken"] = googleSignIn.RegistrationToken,
                ["email"] = googleSignIn.Email,
                ["fullName"] = googleSignIn.FullName
            }));

        return Redirect(BuildGoogleFrontendRedirect("success", new Dictionary<string, string?>
        {
            ["token"] = googleSignIn.Token,
            ["email"] = googleSignIn.Email,
            ["fullName"] = googleSignIn.FullName
        }));
    }

    [HttpPost("google/complete-registration")]
    public async Task<IActionResult> CompleteGoogleRegistration([FromBody] CompleteGoogleRegistrationRequest request)
    {
        var (success, token, email, fullName, error) =
            await _identityService.CompleteGoogleRegistrationAsync(request.RegistrationToken, request.Cedula);

        if (!success)
        {
            if (IsCedulaProviderUnavailable(error))
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = error });

            if (IsDuplicateConflict(error))
                return Conflict(new { message = error });

            return BadRequest(new { message = error });
        }

        return Ok(new AuthResponse(token, email, fullName));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me([FromServices] IUnitOfWork unitOfWork)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Token inválido." });

        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new { message = "Usuario no encontrado." });

        return Ok(new CurrentUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Cedula,
            user.PhoneNumber,
            user.AccountStatus,
            user.IsEmailVerified));
    }

    private bool IsGoogleAuthConfigured()
    {
        return !string.IsNullOrWhiteSpace(_config["Authentication:Google:ClientId"]) &&
               !string.IsNullOrWhiteSpace(_config["Authentication:Google:ClientSecret"]);
    }

    private string BuildGoogleFrontendRedirect(string status, Dictionary<string, string?> values)
    {
        var callbackUrl = _config["Authentication:Google:FrontendCallbackUrl"]
            ?? "http://localhost:4200/auth/google/callback";

        values["status"] = status;
        return QueryHelpers.AddQueryString(callbackUrl, values);
    }

    private static bool IsCedulaProviderUnavailable(string error)
    {
        return string.Equals(
            error,
            "No fue posible validar la cedula en este momento.",
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDuplicateConflict(string error)
    {
        return error.Contains("registrad", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCedulaNotFound(string error)
    {
        return string.Equals(
            error,
            "La cédula no existe en el padrón electoral.",
            StringComparison.OrdinalIgnoreCase);
    }
}
