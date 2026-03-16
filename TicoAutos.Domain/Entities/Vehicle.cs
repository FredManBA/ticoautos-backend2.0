// TicoAutos.Domain/Entities/Vehicle.cs
using TicoAutos.Domain.Common;

namespace TicoAutos.Domain.Entities;

public class Vehicle : BaseEntity
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsSold { get; set; } = false;

    // Owner relationship (FK)
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}