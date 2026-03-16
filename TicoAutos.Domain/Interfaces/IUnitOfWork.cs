using TicoAutos.Domain.Interfaces;


/// <summary>
/// Represents the Unit of Work contract within the application's persistence layer,
/// following the Clean Architecture principles.
/// 
/// This interface coordinates the work of multiple repositories by ensuring that
/// all changes across them are committed as a single transaction. It provides a
/// centralized entry point for repository access and guarantees consistency of
/// data operations.
/// 
/// Implementations of this interface are responsible for managing the lifecycle
/// of the underlying data context.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IVehicleRepository Vehicles { get; }
    IUserRepository Users { get; }
    IQuestionRepository Questions { get; }
    IAnswerRepository Answers { get; }
    Task<int> SaveChangesAsync();
}