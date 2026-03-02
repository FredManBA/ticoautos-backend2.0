namespace TicoAutos.Application.DTOs.Vehicles;


/// <summary>
/// Methods for updating an existing vehicle, encapsulating the necessary properties to update a vehicle entity in the application layer, following Clean Architecture principles.
/// </summary>
/// <param name="Id"></param>
/// <param name="Brand"></param>
/// <param name="Model"></param>
/// <param name="Year"></param>
/// <param name="Price"></param>
/// <param name="Description"></param>
/// <param name="ImageUrl"></param>
/// <param name="IsSold"></param>
public record UpdateVehicleRequest(
    int Id, 
    string Brand,
    string Model,
    int Year,
    decimal Price,
    string Description,
    string ImageUrl,
    bool IsSold
);