// Record type to represent a login request with email and password properties
namespace TicoAutos.Application.DTOs;

public record LoginRequest(string Email, string Password);