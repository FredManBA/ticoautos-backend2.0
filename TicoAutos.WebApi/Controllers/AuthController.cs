using Microsoft.AspNetCore.Mvc;
using TicoAutos.Application.DTOs;
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
    /// Authenticates a user and returns a JWT token.
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
    /// Registers a new user and returns a JWT token.
    /// POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, token, error) = await _identityService.RegisterAsync(
            request.Email, request.Password, request.FullName);

        if (!success)
            return BadRequest(new { message = error });

        return Ok(new AuthResponse(token, request.Email, request.FullName));
    }
}