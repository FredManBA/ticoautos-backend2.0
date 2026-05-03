using Microsoft.EntityFrameworkCore;
using TicoAutos.Domain.Entities;
using TicoAutos.Domain.Interfaces;
using TicoAutos.Infrastructure.Persistence;

namespace TicoAutos.Infrastructure.Repositories;

/// <summary>
/// Concrete implementation of the user repository for data access operations.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByExternalLoginAsync(string authProvider, string externalProviderId) =>
        await _context.Users.FirstOrDefaultAsync(u =>
            u.AuthProvider == authProvider && u.ExternalProviderId == externalProviderId);

    public async Task<User?> GetByEmailVerificationTokenAsync(string token) =>
        await _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

    public async Task<bool> ExistsAsync(string email) =>
        await _context.Users.AnyAsync(u => u.Email == email);

    public async Task<bool> ExistsByCedulaAsync(string cedula) =>
        await _context.Users.AnyAsync(u => u.Cedula == cedula);

    public async Task AddAsync(User user) =>
        await _context.Users.AddAsync(user);
}
