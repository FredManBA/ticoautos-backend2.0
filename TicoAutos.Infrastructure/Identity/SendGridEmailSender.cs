using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using TicoAutos.Domain.Interfaces;

namespace TicoAutos.Infrastructure.Identity;

/// <summary>
/// Sends transactional emails through SendGrid's Mail Send API.
/// </summary>
public class SendGridEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SendGridEmailSender(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendVerificationEmailAsync(
        string recipientEmail,
        string recipientName,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];
        var fromEmail = _configuration["SendGrid:FromEmail"];
        var fromName = _configuration["SendGrid:FromName"] ?? "TicoAutos";

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SendGrid:ApiKey is missing.");

        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException("SendGrid:FromEmail is missing.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "mail/send");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            personalizations = new[]
            {
                new
                {
                    to = new[]
                    {
                        new { email = recipientEmail, name = recipientName }
                    },
                    subject = "Verifica tu cuenta de TicoAutos"
                }
            },
            from = new { email = fromEmail, name = fromName },
            content = new[]
            {
                new
                {
                    type = "text/plain",
                    value = $"Hola {recipientName}, verifica tu cuenta en el siguiente enlace: {verificationLink}"
                }
            }
        });

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"SendGrid rejected the verification email: {(int)response.StatusCode} {body}");
        }
    }
}
