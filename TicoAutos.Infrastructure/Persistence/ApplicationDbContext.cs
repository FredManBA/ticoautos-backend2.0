using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TicoAutos.Domain.Entities;

namespace TicoAutos.Infrastructure.Persistence;

/// <summary>
/// Represents the application's database context for Entity Framework Core.
/// 
/// This class acts as the primary gateway between the domain entities and the
/// underlying database, managing entity sets, configurations, and persistence
/// operations within the Infrastructure layer, following Clean Architecture principles.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext with the specified options.
    /// </summary>
    /// <param name="options">The options used to configure the database context.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    /// <summary>
    /// Gets the database set for vehicle entities, enabling CRUD operations.
    /// </summary>
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    /// <summary>
    /// Gets the database set for user entities, enabling CRUD operations and user management.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Gets the database set for question entities.
    /// </summary>
    public DbSet<Question> Questions => Set<Question>();

    /// <summary>
    /// Gets the database set for answer entities.
    /// </summary>
    public DbSet<Answer> Answers => Set<Answer>();

    /// <summary>
    /// Configures entity mappings and database schema rules using the Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the entity model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.Price)
            .HasPrecision(18, 2);
    }
}