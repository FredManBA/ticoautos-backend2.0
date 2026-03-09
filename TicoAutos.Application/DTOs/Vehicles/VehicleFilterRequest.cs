namespace TicoAutos.Application.DTOs.Vehicles;

/// <summary>
/// Query parameters for filtering and paginating vehicle search results.
/// </summary>
public class VehicleFilterRequest
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsSold { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}