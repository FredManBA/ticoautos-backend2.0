using TicoAutos.Domain.Entities;

namespace TicoAutos.Domain.Interfaces;

/// <summary>
/// Contract for the Vehicle repository, defining the operations for managing vehicle entities
/// </summary>
public interface IVehicleRepository
{
    Task<IEnumerable<Vehicle>> GetAllAsync();
    Task<Vehicle?> GetByIdAsync(int id);
    IQueryable<Vehicle> GetQueryable();
    Task AddAsync(Vehicle vehicle);
    void Update(Vehicle vehicle);
    void Delete(Vehicle vehicle);
}