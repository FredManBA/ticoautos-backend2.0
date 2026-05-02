using Microsoft.EntityFrameworkCore;
using TicoAutos.Domain.Entities;
using TicoAutos.Domain.Interfaces;
using TicoAutos.Infrastructure.Persistence;

namespace TicoAutos.Infrastructure.Repositories;


/// <summary>
/// Repository for managing questions related to vehicles, including adding new questions and retrieving questions by vehicle ID. This implementation uses Entity Framework Core for data access operations.
/// </summary>
public class QuestionRepository : IQuestionRepository
{
    private readonly ApplicationDbContext _context;

    public QuestionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Method to add a new question to the database. This method takes a Question object as a parameter and adds it to the Questions DbSet in the ApplicationDbContext. 
    /// </summary>
    /// <param name="question"></param>
    /// <returns></returns>
    public async Task AddAsync(Question question) =>
        await _context.Questions.AddAsync(question);


    /// <summary>
    /// Method to retrieve a question by its ID, including the related vehicle and answer information.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Question?> GetByIdWithVehicleAsync(int id) =>
        await _context.Questions
            .Include(q => q.Vehicle)
            .Include(q => q.Answer)
            .FirstOrDefaultAsync(q => q.Id == id);


    /// <summary>
    /// Method to retrieve all questions associated with a specific vehicle ID, including the asker and answer information. The results are ordered by the creation date of the questions.
    /// </summary>
    /// <param name="vehicleId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Question>> GetByVehicleIdAsync(int vehicleId) =>
        await _context.Questions
            .Include(q => q.Asker)
            .Include(q => q.Answer)
            .Where(q => q.VehicleId == vehicleId)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Question>> GetByAskerIdAsync(int askerId) =>
        await _context.Questions
            .Include(q => q.Vehicle)
                .ThenInclude(v => v.Owner)
            .Include(q => q.Answer)
            .Where(q => q.AskerId == askerId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Question>> GetByOwnerIdAsync(int ownerId) =>
        await _context.Questions
            .Include(q => q.Asker)
            .Include(q => q.Vehicle)
            .Include(q => q.Answer)
            .Where(q => q.Vehicle.OwnerId == ownerId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Question>> GetByVehicleIdForOwnerAsync(int vehicleId, int ownerId) =>
        await _context.Questions
            .Include(q => q.Asker)
            .Include(q => q.Vehicle)
            .Include(q => q.Answer)
            .Where(q => q.VehicleId == vehicleId && q.Vehicle.OwnerId == ownerId)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();
}
