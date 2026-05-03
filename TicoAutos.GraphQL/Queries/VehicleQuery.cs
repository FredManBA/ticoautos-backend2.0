using Microsoft.EntityFrameworkCore;
using TicoAutos.Domain.Interfaces;
using TicoAutos.GraphQL.Types;

namespace TicoAutos.GraphQL.Queries;

/// <summary>
/// GraphQL queries for Vehicle entity.
/// Uses IUnitOfWork following Clean Architecture — no direct DbContext access.
/// </summary>
[QueryType]
public class VehicleQuery
{
    /// <summary>
    /// Returns a paginated and filtered list of vehicles.
    /// </summary>
    public async Task<IReadOnlyList<VehicleType>> GetVehicles(
        [Service] IUnitOfWork unitOfWork,
        string? brand = null,
        string? model = null,
        int? minYear = null,
        int? maxYear = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? isSold = null,
        int page = 1,
        int pageSize = 10)
    {
        var normalizedPage = Math.Max(page, 1);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 100);

        var query = unitOfWork.Vehicles.GetQueryable()
            .AsNoTracking()
            .Include(v => v.Owner)
            .Include(v => v.Questions)
                .ThenInclude(q => q.Answer)
            .AsQueryable();

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

        return vehicles.Select(v => new VehicleType
        {
            Id = v.Id,
            Brand = v.Brand,
            Model = v.Model,
            Year = v.Year,
            Price = v.Price,
            Description = v.Description,
            ImageUrl = v.ImageUrl,
            IsSold = v.IsSold,
            CreatedAt = v.CreatedAt,
            OwnerId = v.OwnerId,
            OwnerName = v.Owner?.Name ?? "Vendedor",
            UnansweredQuestions = v.Questions.Count(q => q.Answer is null),
            Questions = v.Questions.Select(q => new QuestionType
            {
                Id = q.Id,
                Content = q.Content,
                CreatedAt = q.CreatedAt,
                VehicleId = q.VehicleId,
                AskerId = q.AskerId,
                AskerName = q.Asker?.Name ?? "Usuario",
                Answer = q.Answer is null ? null : new AnswerType
                {
                    Id = q.Answer.Id,
                    Content = q.Answer.Content,
                    CreatedAt = q.Answer.CreatedAt
                }
            }).ToList()
        }).ToList();
    }

    /// <summary>
    /// Returns a single vehicle by ID.
    /// </summary>
    public async Task<VehicleType?> GetVehicle(
        [Service] IUnitOfWork unitOfWork,
        int id)
    {
        var v = await unitOfWork.Vehicles.GetQueryable()
            .AsNoTracking()
            .Include(v => v.Owner)
            .Include(v => v.Questions)
                .ThenInclude(q => q.Answer)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (v is null) return null;

        return new VehicleType
        {
            Id = v.Id,
            Brand = v.Brand,
            Model = v.Model,
            Year = v.Year,
            Price = v.Price,
            Description = v.Description,
            ImageUrl = v.ImageUrl,
            IsSold = v.IsSold,
            CreatedAt = v.CreatedAt,
            OwnerId = v.OwnerId,
            OwnerName = v.Owner?.Name ?? "Vendedor",
            UnansweredQuestions = v.Questions.Count(q => q.Answer is null),
            Questions = v.Questions.Select(q => new QuestionType
            {
                Id = q.Id,
                Content = q.Content,
                CreatedAt = q.CreatedAt,
                VehicleId = q.VehicleId,
                AskerId = q.AskerId,
                AskerName = q.Asker?.Name ?? "Usuario",
                Answer = q.Answer is null ? null : new AnswerType
                {
                    Id = q.Answer.Id,
                    Content = q.Answer.Content,
                    CreatedAt =