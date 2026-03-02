namespace TicoAutos.Application.DTOs.Vehicles;

/// <summary>
/// DTOs for transferring vehicle data from the application layer to the presentation layer, encapsulating the properties of a vehicle in a format suitable for API responses or UI rendering,
/// following Clean Architecture principles.
/// </summary>
/// <param name="Id"></param>
/// <param name="Brand"></param>
/// <param name="Model"></param>
/// <param name="Year"></param>
/// <param name="Price"></param>
/// <param name="Description"></param>
/// <param name="ImageUrl"></param>
/// <param name="IsSold"></param>
/// <param name="CreatedAt"></param>
public record VehicleResponseDto(
    int Id,
    string Brand,
    string Model,
    int Year,
    decimal Price,
    string Description,
    string ImageUrl,
    bool IsSold,
    DateTime CreatedAt
);