namespace TicoAutos.Application.DTOs;

/// <summary>
/// Represents the authentication response returned after login or registration.
/// </summary>
public record AuthResponse(string Token, string Email, string FullName);