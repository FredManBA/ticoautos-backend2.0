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
    Task<(bool Success, string Message, string Error)> ResendVerificationAsync(string email);
    Task<(bool Success, bool RequiresCedula, string Token, string RegistrationToken, string Email, string FullName, string Error)> GoogleSignInAsync(string email, string fullName, string googleId);
    Task<(bool Success, string Token, string Email, string FullName, string Error)> CompleteGoogleRegistrationAsync(string registrationToken, string cedula);
}
