using TicoAutos.Domain.Entities;

namespace TicoAutos.Domain.Interfaces;

/// <summary>
/// Contract for managing answers related to questions about vehicles, including adding new answers.
/// </summary>
public interface IAnswerRepository
{
    Task AddAsync(Answer answer);
}