using TicoAutos.Domain.Entities;
using TicoAutos.Domain.Interfaces;
using TicoAutos.Infrastructure.Persistence;

namespace TicoAutos.Infrastructure.Repositories;

public class AnswerRepository : IAnswerRepository
{
    private readonly ApplicationDbContext _context;

    public AnswerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Answer answer) =>
        await _context.Answers.AddAsync(answer);
}