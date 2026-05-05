using Microsoft.EntityFrameworkCore;
using TicoAutos.Domain.Interfaces;
using TicoAutos.GraphQL.Types;

namespace TicoAutos.GraphQL.Queries;

[QueryType]
public class VehicleQuery
{
    public IReadOnlyList<VehicleType> GetVehiclesTest()
    {
        return new List<VehicleType>
        {
            new VehicleType { Id = 1, Brand = "Toyota", Model = "Corolla", Year = 2022, Price = 15000 }
        };
    }

    public async Task<IReadOnlyList<VehicleType>> GetVehicles(
     [Service] IUnitOfWork unitOfWork,
     [Service] ILogger<VehicleQuery> logger,
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
        try
        {
            var vehicles = await unitOfWork.Vehicles.GetQueryable()
                .AsNoTracking()
                .Include(v => v.Owner)
                .Take(5)
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
                UnansweredQuestions = 0,
                Questions = new List<QuestionType>()
            }).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error en GetVehicles: {Message}", ex.Message);
            throw;
        }
    }

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
                    CreatedAt = q.Answer.CreatedAt
                }
            }).ToList()
        };
    }
}
