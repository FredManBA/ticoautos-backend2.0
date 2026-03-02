namespace TicoAutos.Domain.Common;

/// <summary>
/// Base class for all domain entities, providing common properties 
/// like unique identification and auditing timestamps.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}