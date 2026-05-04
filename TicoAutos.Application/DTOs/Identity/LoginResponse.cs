namespace TicoAutos.Application.DTOs;

public record LoginResponse(
    bool RequiresTwoFactor,
    string? Token,
    string? TemporaryToken,
    string Email,
    string FullName,
    string Message);
