

namespace TicoAutos.Application.DTOs;

/// <summary>
/// DTO returned after a successful registration.
/// </summary>
public record UserResponseDto(int Id, string FullName, string Email, string Token);