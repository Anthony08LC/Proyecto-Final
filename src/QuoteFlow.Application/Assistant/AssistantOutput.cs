using System.Text;
using System.Text.RegularExpressions;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Assistant;

/// <summary>
/// Normaliza la respuesta del modelo como texto sin formato: quita bloques de código, marcas de
/// Markdown, comillas envolventes y caracteres de control, y la recorta al máximo del campo.
/// </summary>
internal static partial class AssistantOutput
{
    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var text = raw.Replace("\r\n", "\n").Replace('\r', '\n');
        text = CodeFence().Replace(text, string.Empty);
        text = Heading().Replace(text, string.Empty);
        text = Emphasis().Replace(text, "$2");
        text = Bullet().Replace(text, "- ");
        text = RemoveControlCharacters(text);
        text = ExtraBlankLines().Replace(text, "\n\n").Trim();
        text = text.Trim('"', '“', '”', '«', '»').Trim();

        if (text.Length == 0)
        {
            return null;
        }

        return text.Length <= Quote.TextMaxLength ? text : text[..Quote.TextMaxLength].TrimEnd();
    }

    private static string RemoveControlCharacters(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var character in text)
        {
            if (character == '\n' || character == '\t' || !char.IsControl(character))
            {
                builder.Append(character);
            }
        }
        return builder.ToString();
    }

    [GeneratedRegex(@"^\s*```[^\n]*\n?", RegexOptions.Multiline)]
    private static partial Regex CodeFence();

    [GeneratedRegex(@"^\s{0,3}#{1,6}\s+", RegexOptions.Multiline)]
    private static partial Regex Heading();

    [GeneratedRegex(@"(\*\*|__|\*)(\S(?:.*?\S)?)\1")]
    private static partial Regex Emphasis();

    [GeneratedRegex(@"^\s*[*•]\s+", RegexOptions.Multiline)]
    private static partial Regex Bullet();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExtraBlankLines();
}
