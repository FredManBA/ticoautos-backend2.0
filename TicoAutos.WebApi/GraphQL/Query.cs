using Microsoft.EntityFrameworkCore;
using TicoAutos.Domain.Entities;
using TicoAutos.Infrastructure.Persistence;

namespace TicoAutos.WebApi.GraphQL;

public class Query
{
    public async Task<IReadOnlyList<VehicleGraphQlDto>> GetVehicles(
        [Service] ApplicationDbContext dbContext,
        string? brand,
        string? model,
        int? minYear,
        int? maxYear,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isSold,
        int page = 1,
        int pageSize = 10)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Vehicle> query = dbContext.Vehicles
            .AsNoTracking()
            .Include(v => v.Owner)
            .Include(v => v.Questions)
                .ThenInclude(q => q.Answer);

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(v => v.Brand.ToLower().Contains(brand.ToLower()));

        if (!string.IsNullOrWhiteSpace(model))
            query = query.Where(v => v.Model.ToLower().Contains(model.ToLower()));

        if (minYear.HasValue)
            query = query.Where(v => v.Year >= minYear.Value);

        if (maxYear.HasValue)
            query = query.Where(v => v.Year <= maxYear.Value);

        if (minPrice.HasValue)
            query = query.Where(v => v.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(v => v.Price <= maxPrice.Value);

        if (isSold.HasValue)
            query = query.Where(v => v.IsSold == isSold.Value);

        var vehicles = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        return vehicles.Select(MapVehicle).ToList();
    }

    public async Task<VehicleGraphQlDto?> GetVehicle([Service] ApplicationDbContext dbContext, int id)
    {
        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .Include(v => v.Owner)
            .Include(v => v.Questions)
                .ThenInclude(q => q.Answer)
            .FirstOrDefaultAsync(v => v.Id == id);

        return vehicle is null ? null : MapVehicle(vehicle);
    }

    public async Task<IReadOnlyList<QuestionGraphQlDto>> GetQuestionsByVehicle(
        [Service] ApplicationDbContext dbContext,
        int vehicleId)
    {
        var questions = await dbContext.Questions
            .AsNoTracking()
            .Include(q => q.Asker)
            .Include(q => q.Answer)
            .Where(q => q.VehicleId == vehicleId)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();

        return questions.Select(MapQuestion).ToList();
    }

    private static VehicleGraphQlDto MapVehicle(Vehicle vehicle)
    {
        return new VehicleGraphQlDto(
            vehicle.Id,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.Price,
            vehicle.Description,
            vehicle.ImageUrl,
            vehicle.IsSold,
            vehicle.CreatedAt,
            vehicle.OwnerId,
            vehicle.Owner?.Name ?? "Vendedor",
            vehicle.Questions.Count(q => q.Answer is null));
    }

    private static QuestionGraphQlDto MapQuestion(Question question)
    {
        return new QuestionGraphQlDto(
            question.Id,
            question.Content,
            question.CreatedAt,
            question.VehicleId,
            question.AskerId,
            question.Asker?.Name ?? "Usuario",
            question.Answer is null
                ? null
                : new AnswerGraphQlDto(question.Answer.Id, question.Answer.Content, question.Answer.CreatedAt));
    }
}
