using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using TicoAutos.Domain.Interfaces;

namespace TicoAutos.Infrastructure.Identity;

public class TwilioVerifyTwoFactorSender : ITwoFactorSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public TwilioVerifyTwoFactorSender(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> SendCodeAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var serviceSid = GetRequiredSetting("Twilio:VerifyServiceSid");

        using var request = CreateRequest(
            HttpMethod.Post,
            $"Services/{serviceSid}/Verifications",
            new Dictionary<string, string>
            {
                ["To"] = phoneNumber,
                ["Channel"] = "sms"
            });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CheckCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        var serviceSid = GetRequiredSetting("Twilio:VerifyServiceSid");

        using var request = CreateRequest(
            HttpMethod.Post,
            $"Services/{serviceSid}/VerificationCheck",
            new Dictionary<string, string>
            {
                ["To"] = phoneNumber,
                ["Code"] = code
            });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<TwilioVerificationCheckResponse>(cancellationToken);
        return result?.Status == "approved";
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, Dictionary<string, string> form)
    {
        var accountSid = GetRequiredSetting("Twilio:AccountSid");
        var authToken = GetRequiredSetting("Twilio:AuthToken");

        var request = new HttpRequestMessage(method, path)
        {
            Content = new FormUrlEncodedContent(form)
        };

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        return request;
    }

    private string GetRequiredSetting(string key)
    {
        return _configuration[key]
            ?? throw new InvalidOperationException($"{key} is missing.");
    }

    private sealed class TwilioVerificationCheckResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}
