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
    /// Method to authenticate a user and generate a JWT token. In a real application, this would validate the user's credentials against a database and return a token if valid.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Simulation: In a real application, you would validate the user's credentials against a database or an identity provider.
        if (request.Email == "admin@ticoautos.com" && request.Password == "P@ssword123")
        {
            var token = _identityService.GenerateToken(request.Email, Guid.NewGuid().ToString());
            return Ok(new { Token = token });
        }

        return Unauthorized("Credenciales inválidas");
    }
}