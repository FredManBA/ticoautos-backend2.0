namespace TicoAutos.Domain.Interfaces;

/// <summary>
/// Validates Costa Rican identity numbers against an external electoral registry service.
/// </summary>
public interface ICedulaValidationService
{
    Task<CedulaValidationResult> ValidateAsync(string cedula, CancellationToken cancellationToken = default);
}

public sealed record CedulaValidationResult(
    bool IsValid,
    string Cedula,
    string FullName,
    string? Error);
