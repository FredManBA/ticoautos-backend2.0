namespace TicoAutos.Domain.Entities;

/// <summary>
/// Identifies a question asked by a potential buyer about a specific vehicle, encapsulating its properties and relationships within the domain model of the TicoAutos application.
/// </summary>
public class Question
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    // One Question has one Answer (Optional)
    public Answer? Answer { get; set; }
}