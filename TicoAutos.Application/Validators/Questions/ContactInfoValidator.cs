using System.Text.RegularExpressions;

namespace TicoAutos.Application.Validators.Questions;

internal static partial class ContactInfoValidator
{
    public const string Message = "No se permite compartir datos de contacto en preguntas o respuestas.";

    public static bool DoesNotContainContactInfo(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return true;

        return !EmailRegex().IsMatch(content) &&
               !PhoneRegex().IsMatch(content) &&
               !ExternalLinkRegex().IsMatch(content);
    }

    [GeneratedRegex(@"[\w.%+-]+@[\w.-]+\.[A-Za-z]{2,}", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"(?<!\d)(?:\+?506[\s.-]?)?\d{4}[\s.-]?\d{4}(?!\d)")]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"(?:https?://|www\.|wa\.me/)", RegexOptions.IgnoreCase)]
    private static partial Regex ExternalLinkRegex();
}
