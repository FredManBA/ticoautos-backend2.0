namespace TicoAutos.Application.DTOs;

public record VerifyTwoFactorRequest(string TemporaryToken, string Code);
