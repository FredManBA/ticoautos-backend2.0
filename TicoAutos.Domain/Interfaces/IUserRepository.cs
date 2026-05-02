using TicoAutos.Domain.Entities;

namespace TicoAutos.Domain.Interfaces;

/// <summary>
/// Contract for user data access operations.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailVerificationTokenAsync(string token);
    Task<bool> ExistsAsync(string email);
    Task<bool> ExistsByCedulaAsync(string cedula);
    Task AddAsync(User user);
}
