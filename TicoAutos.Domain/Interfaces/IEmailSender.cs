namespace TicoAutos.Domain.Interfaces;

/// <summary>
/// Sends transactional emails through the configured external provider.
/// </summary>
public interface IEmailSender
{
    Task SendVerificationEmailAsync(
        string recipientEmail,
        string recipientName,
        string verificationLink,
        CancellationToken cancellationToken = default);
}
