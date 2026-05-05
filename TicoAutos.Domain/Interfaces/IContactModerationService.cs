namespace TicoAutos.Domain.Interfaces;

/// <summary>
/// Validates marketplace messages before they are saved or shown to other users.
/// </summary>
public interface IContactModerationService
{
    Task<ContactModerationResult> ValidateAsync(string content, CancellationToken cancellationToken = default);
}

public sealed record ContactModerationResult(
    bool IsAllowed,
    string Message,
    IReadOnlyCollection<string> DetectedTypes)
{
    public static ContactModerationResult Allowed() =>
        new(true, string.Empty, Array.Empty<string>());

    public static ContactModerationResult Blocked(string message, IReadOnlyCollection<string> detectedTypes) =>
        new(false, message, detectedTypes);
}
