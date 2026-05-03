namespace TicoAutos.GraphQL.Types;

/// <summary>
/// GraphQL type definition for Vehicle.
/// Maps the Vehicle domain entity to a GraphQL type with its exposed fields.
/// </summary>
public class VehicleType
{
    public int Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsSold { get; set; }
    public DateTime CreatedAt { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int UnansweredQuestions { get; set; }
    public IReadOnlyList<QuestionType> Questions { get; set; } = [];
}