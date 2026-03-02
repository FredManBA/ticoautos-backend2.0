using Microsoft.EntityFrameworkCore;
using TicoAutos.Domain.Entities;
using TicoAutos.Domain.Interfaces;
using TicoAutos.Infrastructure.Persistence;

namespace TicoAutos.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly ApplicationDbContext _context;

    public VehicleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Methods for CRUD operations on Vehicle entity (get all, get by id, add, update, delete).
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<Vehicle>> GetAllAsync() =>
        await _context.Vehicles.ToListAsync();

    public async Task<Vehicle?> GetByIdAsync(int id) =>
        await _context.Vehicles.Include(v => v.Questions).FirstOrDefaultAsync(v => v.Id == id);

    public async Task AddAsync(Vehicle vehicle) =>
        await _context.Vehicles.AddAsync(vehicle);

    public void Update(Vehicle vehicle) => _context.Vehicles.Update(vehicle);

    public void Delete(Vehicle vehicle) => _context.Vehicles.Remove(vehicle);
}