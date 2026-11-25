using System.Text.RegularExpressions;

namespace QuoteFlow.Application.Assistant;

/// <summary>
/// Quita correos y teléfonos de los textos libres que se envían al modelo (RNF-SEG-11): textos
/// comerciales, descripciones de productos y condiciones predeterminadas. Los datos de contacto
/// estructurados (del cliente y de la empresa) ni siquiera se incluyen en el contexto.
/// </summary>
internal static partial class SensitiveDataFilter
{
    public const string EmailPlaceholder = "[correo omitido]";
    public const string PhonePlaceholder = "[teléfono omitido]";

    public static string Redact(string text)
    {
        var withoutEmails = EmailPattern().Replace(text, EmailPlaceholder);
        return PhonePattern().Replace(withoutEmails, PhonePlaceholder);
    }

    // usuario@dominio.tld
    [GeneratedRegex(@"[\p{L}\p{N}._%+\-]+@[\p{L}\p{N}\-]+(?:\.[\p{L}\p{N}\-]+)*\.\p{L}{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex EmailPattern();

    // Internacionales con prefijo (+506 8888-7777, +1 305 555 0100) y locales de 8 dígitos
    // (8888-7777, 8888 7777, 88887777, (506) 2222-3333). No toca montos como ₡1.250.000,00.
    [GeneratedRegex(
        @"(?<![\p{N}₡.,])(?:\+\d{1,3}[\s.\-]?(?:\(\d{1,4}\)[\s.\-]?)?\d(?:[\s.\-]?\d){6,12}|(?:\(506\)[\s.\-]?|506[\s.\-])?\d{4}[\s\-]?\d{4})(?![\p{N}]|[.,]\d)",
        RegexOptions.CultureInvariant)]
    private static partial Regex PhonePattern();
}
