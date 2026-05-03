namespace TicoAutos.Application.DTOs;

public record RegisterRequest(string Email, string Password, string Cedula, string PhoneNumber);
