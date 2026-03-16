namespace TicoAutos.Application.DTOs.Vehicles;

/// <summary>
/// DTO for transferring vehicle data from the application layer to the presentation layer,
/// encapsulating the properties of a vehicle in a format suitable for API responses,
/// following Clean Architecture principles.
/// </summary>
public record VehicleResponseDto(
    int Id,
    string Brand,
    string Model,
    int Year,
    decimal Price,
    string Description,
    string ImageUrl,
    bool IsSold,
    DateTime CreatedAt,
    int OwnerId,
    string OwnerName
);