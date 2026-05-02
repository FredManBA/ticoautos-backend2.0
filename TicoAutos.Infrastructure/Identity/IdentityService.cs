using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TicoAutos.Domain.Entities;
using TicoAutos.Domain.Interfaces;

namespace TicoAutos.Infrastructure.Identity;

/// <summary>
/// Handles JWT token generation, user registration and login logic.
/// </summary>
public class IdentityService : IIdentityService
{
    private readonly IConfiguration _config;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICedulaValidationService _cedulaValidationService;

    public IdentityService(
        IConfiguration config,
        IUnitOfWork unitOfWork,
        ICedulaValidationService cedulaValidationService)
    {
        _config = config;
        _unitOfWork = unitOfWork;
        _cedulaValidationService = cedulaValidationService;
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
    /// Registers a new user after validating email uniqueness and hashing the password.
    /// </summary>
    public async Task<(bool Success, int UserId, string Token, string FullName, string Error)> RegisterAsync(
        string email, string password, string cedula)
    {

        if (await _unitOfWork.Users.ExistsAsync(email))
            return (false, 0, string.Empty, string.Empty, "Email already registered.");

        if (await _unitOfWork.Users.ExistsByCedulaAsync(cedula))
            return (false, 0, string.Empty, string.Empty, "Cedula already registered.");

        var cedulaValidation = await _cedulaValidationService.ValidateAsync(cedula);
        if (!cedulaValidation.IsValid)
            return (false, 0, string.Empty, string.Empty, cedulaValidation.Error ?? "Cedula validation failed.");

        var user = new User
        {
            Email = email,
            Name = cedulaValidation.FullName,
            Cedula = cedulaValidation.Cedula,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var token = GenerateToken(user.Email, user.Id.ToString());
        return (true, user.Id, token, user.Name, string.Empty);
    }

    /// <summary>
    /// Validates user credentials and returns a JWT token if successful.
    /// </summary>
    public async Task<(bool Success, string Token, string Error)> LoginAsync(
        string email, string password)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, string.Empty, "Invalid credentials.");

        var token = GenerateToken(user.Email, user.Id.ToString());
        return (true, token, string.Empty);
    }
}
