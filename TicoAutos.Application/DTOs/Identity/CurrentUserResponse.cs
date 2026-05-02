namespace TicoAutos.Application.DTOs;

public record CurrentUserResponse(
    int Id,
    string FullName,
    string Email,
    string? Cedula,
    string AccountStatus,
    bool IsEmailVerified);
