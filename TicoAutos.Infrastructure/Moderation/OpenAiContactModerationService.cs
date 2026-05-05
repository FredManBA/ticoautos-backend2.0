using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TicoAutos.Domain.Interfaces;

namespace TicoAutos.Infrastructure.Moderation;

public sealed partial class OpenAiContactModerationService : IContactModerationService
{
    private const string BlockedMessage = "No se permite compartir información de contacto. Use la plataforma para comunicarse.";
    private const string ValidationUnavailableMessage = "No fue posible validar el mensaje en este momento.";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiContactModerationService> _logger;

    public OpenAiContactModerationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenAiContactModerationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ContactModerationResult> ValidateAsync(string content, CancellationToken cancellationToken = default)
    {
        var localResult = ValidateWithLocalRules(content);
        if (!localResult.IsAllowed)
            return localResult;

        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return ContactModerationResult.Blocked(ValidationUnavailableMessage, new[] { "openai_not_configured" });

        var model = _configuration["OpenAI:Model"];
        if (string.IsNullOrWhiteSpace(model))
            model = "gpt-5.4-mini";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "responses")
            {
                Content = JsonContent.Create(BuildRequestBody(model, content), options: JsonOptions)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "OpenAI contact moderation failed with status code {StatusCode}.",
                    (int)response.StatusCode);

                return ContactModerationResult.Blocked(ValidationUnavailableMessage, new[] { "openai_unavailable" });
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ResponsesApiResponse>(JsonOptions, cancellationToken);
            var rawDecision = apiResponse?.GetOutputText();
            if (string.IsNullOrWhiteSpace(rawDecision))
                return ContactModerationResult.Blocked(ValidationUnavailableMessage, new[] { "openai_invalid_response" });

            var decision = JsonSerializer.Deserialize<ContactModerationDecision>(rawDecision, JsonOptions);
            if (decision is null)
                return ContactModerationResult.Blocked(ValidationUnavailableMessage, new[] { "openai_invalid_response" });

            return decision.IsAllowed
                ? ContactModerationResult.Allowed()
                : ContactModerationResult.Blocked(
                    BlockedMessage,
                    decision.DetectedTypes.Length == 0 ? new[] { "contact_info" } : decision.DetectedTypes);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenAI contact moderation request failed.");
            return ContactModerationResult.Blocked(ValidationUnavailableMessage, new[] { "openai_unavailable" });
        }
    }

    private static ContactModerationResult ValidateWithLocalRules(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return ContactModerationResult.Allowed();

        var detectedTypes = new List<string>();
        var normalized = Normalize(content);

        if (EmailRegex().IsMatch(content) || ObfuscatedEmailRegex().IsMatch(normalized))
            detectedTypes.Add("email");

        if (PhoneRegex().IsMatch(content) || ObfuscatedPhoneRegex().IsMatch(normalized))
            detectedTypes.Add("phone");

        if (ExternalLinkRegex().IsMatch(content))
            detectedTypes.Add("external_link");

        if (SocialHandleRegex().IsMatch(content) || SocialNetworkRegex().IsMatch(normalized))
            detectedTypes.Add("social_media");

        if (ContactIntentRegex().IsMatch(normalized))
            detectedTypes.Add("off_platform_contact");

        return detectedTypes.Count == 0
            ? ContactModerationResult.Allowed()
            : ContactModerationResult.Blocked(BlockedMessage, detectedTypes.Distinct().ToArray());
    }

    private static object BuildRequestBody(string model, string content) => new
    {
        model,
        store = false,
        text = new
        {
            format = new
            {
                type = "json_schema",
                name = "contact_moderation_result",
                strict = true,
                schema = new
                {
                    type = "object",
                    additionalProperties = false,
                    properties = new Dictionary<string, object>
                    {
                        ["isAllowed"] = new { type = "boolean" },
                        ["reason"] = new { type = "string" },
                        ["detectedTypes"] = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "string",
                                @enum = new[]
                                {
                                    "phone",
                                    "email",
                                    "whatsapp",
                                    "social_media",
                                    "external_link",
                                    "off_platform_contact",
                                    "obfuscated_contact_info"
                                }
                            }
                        }
                    },
                    required = new[] { "isAllowed", "reason", "detectedTypes" }
                }
            }
        },
        input = new[]
        {
            new
            {
                role = "system",
                content = """
                    You validate marketplace vehicle chat messages.

                    Block the message if it shares or tries to share contact information outside the platform.
                    Block phone numbers, emails, WhatsApp, Telegram, social media usernames, profile links,
                    external links, and obfuscated contact details such as "ocho ocho ocho ocho",
                    "gmail punto com", "arroba gmail", or requests to continue negotiation outside the platform.

                    Allow normal vehicle questions, price negotiation, availability questions, financing questions,
                    and location references that do not include contact information.

                    Return only the JSON object required by the schema.
                    """
            },
            new
            {
                role = "user",
                content
            }
        }
    };

    private static string Normalize(string value) =>
        value
            .ToLowerInvariant()
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u")
            .Replace("ñ", "n");

    [GeneratedRegex(@"[\w.%+-]+@[\w.-]+\.[A-Za-z]{2,}", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\b(?:arroba|at)\b.+\b(?:punto|dot)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ObfuscatedEmailRegex();

    [GeneratedRegex(@"(?<!\d)(?:\+?506[\s.-]?)?\d(?:[\s.-]?\d){7}(?!\d)")]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"\b(?:cero|uno|dos|tres|cuatro|cinco|seis|siete|ocho|nueve)(?:\s+(?:cero|uno|dos|tres|cuatro|cinco|seis|siete|ocho|nueve)){3,}\b")]
    private static partial Regex ObfuscatedPhoneRegex();

    [GeneratedRegex(@"(?:https?://|www\.|wa\.me/|bit\.ly/|t\.me/|facebook\.com|instagram\.com|x\.com|twitter\.com|tiktok\.com)", RegexOptions.IgnoreCase)]
    private static partial Regex ExternalLinkRegex();

    [GeneratedRegex(@"(?<!\w)@[A-Za-z0-9._]{3,}")]
    private static partial Regex SocialHandleRegex();

    [GeneratedRegex(@"\b(?:whatsapp|whats|wsp|telegram|instagram|insta|facebook|face|messenger|tiktok)\b", RegexOptions.IgnoreCase)]
    private static partial Regex SocialNetworkRegex();

    [GeneratedRegex(@"\b(?:llamame|llamarme|escribame|escribeme|contacteme|contactame|fuera de la plataforma|por aparte|por privado|por whatsapp|mi numero|mi correo)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ContactIntentRegex();

    private sealed class ResponsesApiResponse
    {
        [JsonPropertyName("output_text")]
        public string? OutputText { get; set; }

        public List<ResponseOutputItem> Output { get; set; } = new();

        public string? GetOutputText()
        {
            if (!string.IsNullOrWhiteSpace(OutputText))
                return OutputText;

            return Output
                .SelectMany(item => item.Content)
                .FirstOrDefault(content => !string.IsNullOrWhiteSpace(content.Text))
                ?.Text;
        }
    }

    private sealed class ResponseOutputItem
    {
        public List<ResponseOutputContent> Content { get; set; } = new();
    }

    private sealed class ResponseOutputContent
    {
        public string? Content { get; set; }

        public string? Text { get; set; }
    }

    private sealed class ContactModerationDecision
    {
        [JsonPropertyName("isAllowed")]
        public bool IsAllowed { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonPropertyName("detectedTypes")]
        public string[] DetectedTypes { get; set; } = Array.Empty<string>();
    }
}
