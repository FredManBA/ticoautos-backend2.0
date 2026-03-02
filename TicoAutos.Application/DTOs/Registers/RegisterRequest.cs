// This record represents a request to register a new user in the TicoAutos application.
namespace TicoAutos.Application.DTOs;

public record RegisterRequest(string Email, string Password, string FullName);