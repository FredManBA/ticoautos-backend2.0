namespace TicoAutos.Domain.Interfaces;

/// <summary>
/// Contract for authentication and token generation operations.
/// </summary>
public interface IIdentityService
{
    string GenerateToken(string email, string userId);
    Task<(bool Success, int UserId, string FullName, string Error)> RegisterAsync(string email, string password, string cedula);
    Task<(bool Success, string Token, string Error)> LoginAsync(string email, string password);
    Task<(bool Success, string Error)> VerifyEmailAsync(string token);
}
