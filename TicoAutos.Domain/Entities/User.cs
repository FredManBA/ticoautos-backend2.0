using TicoAutos.Domain.Common;

namespace TicoAutos.Domain.Entities;

/// <summary>
/// Represents a registered user in the TicoAutos platform.
/// </summary>
public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}