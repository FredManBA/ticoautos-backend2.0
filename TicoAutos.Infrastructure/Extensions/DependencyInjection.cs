using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicoAutos.Domain.Interfaces;
using TicoAutos.Infrastructure.Identity;
using TicoAutos.Infrastructure.Persistence;
using TicoAutos.Infrastructure.Repositories;

namespace TicoAutos.Infrastructure;

/// <summary>
/// Provides extension methods for registering infrastructure layer services
/// into the application's dependency injection container.
/// 
/// This class centralizes the configuration of persistence-related components,
/// including the database context and repository implementations, in accordance
/// with Clean Architecture principles.
/// </summary>
public static class DependencyInjection
{

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));


        // Register repositories and services for dependency injection
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}