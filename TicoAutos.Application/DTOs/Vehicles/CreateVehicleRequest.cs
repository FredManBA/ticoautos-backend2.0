namespace TicoAutos.Application.DTOs.Vehicles;


/// <summary>
/// DTO for creating a new vehicle, encapsulating the necessary properties to create a vehicle entity in the application layer, following Clean Architecture principles.
/// This DTO is used to transfer data from the presentation layer to the application layer when a new vehicle is being created.
/// </summary>
/// <param name="Brand"></param>
/// <param name="Model"></param>
/// <param name="Year"></param>
/// <param name="Price"></param>
/// <param name="Description"></param>
/// <param name="ImageUrl"></param>
public record CreateVehicleRequest(
    string Brand,
    string Model,
    int Year,
    decimal Price,
    string Description,
    string ImageUrl
);