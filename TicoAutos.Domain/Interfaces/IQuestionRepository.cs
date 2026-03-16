using TicoAutos.Domain.Entities;

namespace TicoAutos.Domain.Interfaces;

/// <summary>
/// Contract for managing questions related to vehicles, including adding new questions and retrieving questions by vehicle ID.
/// </summary>
public interface IQuestionRepository
{
    Task AddAsync(Question question);
    Task<Question?> GetByIdWithVehicleAsync(int id);
    Task<IEnumerable<Question>> GetByVehicleIdAsync(int vehicleId);
}