using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TicoAutos.Domain.Interfaces;

namespace TicoAutos.Infrastructure.Identity;

/// <summary>
/// Client for the jeison-araya/cedulas-costa-rica electoral registry API.
/// </summary>
public class CedulaValidationService : ICedulaValidationService
{
    private const string GenericUnavailableMessage = "No fue posible validar la cedula en este momento.";
    private readonly HttpClient _httpClient;

    public CedulaValidationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CedulaValidationResult> ValidateAsync(string cedula, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"padron/cedula/{cedula}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new CedulaValidationResult(false, cedula, string.Empty, "La cédula no existe en el padrón electoral.");

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return new CedulaValidationResult(false, cedula, string.Empty, "El formato de la cédula no es válido.");

            if (!response.IsSuccessStatusCode)
                return new CedulaValidationResult(false, cedula, string.Empty, GenericUnavailableMessage);

            var persona = await response.Content.ReadFromJsonAsync<PadronPersonaResponse>(cancellationToken);

            if (persona is null)
                return new CedulaValidationResult(false, cedula, string.Empty, "La respuesta del padron no tiene el formato esperado.");

            var fullName = $"{persona.Nombre} {persona.PrimerApellido} {persona.SegundoApellido}".Trim();
            return new CedulaValidationResult(true, persona.Cedula, fullName, null);
        }
        catch (HttpRequestException)
        {
            return new CedulaValidationResult(false, cedula, string.Empty, GenericUnavailableMessage);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new CedulaValidationResult(false, cedula, string.Empty, GenericUnavailableMessage);
        }
    }

    private sealed class PadronPersonaResponse
    {
        [JsonPropertyName("cedula")]
        public string Cedula { get; set; } = string.Empty;

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("primer_apellido")]
        public string PrimerApellido { get; set; } = string.Empty;

        [JsonPropertyName("segundo_apellido")]
        public string SegundoApellido { get; set; } = string.Empty;
    }
}
