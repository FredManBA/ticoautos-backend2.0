// TicoAutos.WebApi/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TicoAutos.Application.DTOs;
using TicoAutos.Domain.Constants;
using TicoAutos.Domain.Interfaces;

namespace TicoAutos.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    /// <summary>
    /// Authenticates a verified user and returns a JWT token.
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, token, error) = await _identityService.LoginAsync(request.Email, request.Password);

        if (!success)
            return Unauthorized(new { message = error });

        return Ok(new AuthResponse(token, request.Email, string.Empty));
    }

    /// <summary>
    /// Registers a new pending user and sends an email verification link.
    /// POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, userId, fullName, error) = await _identityService.RegisterAsync(
            request.Email, request.Password, request.Cedula);

        if (!success)
        {
            if (userId > 0)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = error });

            return Conflict(new { message = error });
        }

        return Ok(new RegistrationResponse(
            userId,
            fullName,
            request.Email,
            AccountStatuses.Pending,
            "Usuario registrado. Revise su correo para activar la cuenta."));
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
            user.AccountStatus,
            user.IsEmailVerified));
    }
}
