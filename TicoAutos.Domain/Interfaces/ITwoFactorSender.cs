namespace TicoAutos.Domain.Interfaces;

public interface ITwoFactorSender
{
    Task<bool> SendCodeAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<bool> CheckCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}
