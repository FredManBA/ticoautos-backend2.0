namespace TicoAutos.Application.DTOs;

/// <summary>
/// Response returned after creating a pending user account.
/// </summary>
public record RegistrationResponse(
    int Id,
    string FullName,
    string Email,
    string Status,
    string Message);
