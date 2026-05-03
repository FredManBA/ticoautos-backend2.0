namespace TicoAutos.Application.DTOs;

public record LoginResponse(
    bool RequiresTwoFactor,
    string? Token,
    string? TwoFactorToken,
    string Email,
    string FullName,
    string Message);
