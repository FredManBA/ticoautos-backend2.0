namespace TicoAutos.Domain.Interfaces;

/// <summary>
/// Contract for the Identity service, defining the operations related to user authentication and token generation.
/// </summary>
public interface IIdentityService
{
    string GenerateToken(string email, string userId);
}