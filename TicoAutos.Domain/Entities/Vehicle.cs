using TicoAutos.Domain.Common;

namespace TicoAutos.Domain.Entities;

/// <summary>
/// Identifies a vehicle available for sale, encapsulating its properties 
/// and relationships within the domain model.
/// </summary>
public class Vehicle : BaseEntity
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsSold { get; set; } = false;
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;

    // Relationship: A vehicle can have many questions
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}