namespace TicoAutos.Domain.Entities;


/// <summary>
/// Identifies a vehicle available for sale, encapsulating its properties and relationships within the domain model of the TicoAutos application.
/// </summary>
public class Vehicle
{
    public int Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsSold { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relationship: A vehicle can have many questions
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}