namespace TicoAutos.Application.DTOs.Vehicles;

/// <summary>
/// DTO for transferring vehicle data from the application layer to the presentation layer,
/// following Clean Architecture principles.
/// </summary>
public class VehicleResponseDto
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
}