namespace TicoAutos.Application.DTOs;

public record VerifyTwoFactorRequest(string TwoFactorToken, string Code);
