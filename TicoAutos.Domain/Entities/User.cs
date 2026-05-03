using TicoAutos.Domain.Common;

using TicoAutos.Domain.Constants;

namespace TicoAutos.Domain.Entities;

/// <summary>
/// Represents a registered user in the TicoAutos platform.
/// </summary>
public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Cedula { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = AccountStatuses.Active;
    public bool IsEmailVerified { get; set; } = true;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
