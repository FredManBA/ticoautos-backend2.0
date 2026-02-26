using TicoAutos.Domain.Interfaces;
using TicoAutos.Infrastructure.Persistence;

namespace TicoAutos.Infrastructure.Repositories;

/// <summary>
/// Concrete implementation of the Unit of Work pattern for the persistence layer.
/// 
/// This class coordinates repository operations and manages the lifetime of the
/// database context, ensuring that all data changes are committed as a single
/// transactional unit in accordance with Clean Architecture principles.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IVehicleRepository? _vehicles;

    /// <summary>
    /// Initializes a new instance of the UnitOfWork with the specified database context.
    /// </summary>
    /// <param name="context">The application database context used to manage data persistence.</param>
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Provides access to the vehicle repository using lazy initialization,
    /// ensuring that the repository is created only when first requested.
    /// </summary>
    public IVehicleRepository Vehicles => _vehicles ??= new VehicleRepository(_context);

    /// <summary>
    /// Asynchronously commits all pending changes tracked by the context to the database
    /// as a single transaction.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// the number of state entries written to the database.
    /// </returns>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Releases the database context resources used by the Unit of Work.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}