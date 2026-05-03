namespace TicoAutos.Application.DTOs;

public record CurrentUserResponse(
    int Id,
    string FullName,
    string Email,
    string? Cedula,
    string? PhoneNumber,
    string AccountStatus,
    bool IsEmailVerified);
