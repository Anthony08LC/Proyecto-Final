using System.Globalization;
using QuoteFlow.Domain.Common;

namespace QuoteFlow.Application.Common.Formatting;

/// <summary>
/// Formatos de Costa Rica para los textos legibles (por ejemplo, el detalle de la auditoría):
/// montos ₡1.179.720,00 y fechas dd/mm/aaaa (RNF-USA-03). No depende de la cultura del servidor.
/// </summary>
internal static class DisplayFormat
{
    private static readonly NumberFormatInfo CostaRicaNumbers = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NumberGroupSizes = [3]
    };

    public static string Colones(decimal amount) =>
        "₡" + amount.ToString("#,##0.00", CostaRicaNumbers);

    public static string Percent(decimal value) =>
        value.ToString("0.##", CostaRicaNumbers) + " %";

    public static string Date(DateOnly date) =>
        date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    public static string Date(DateTimeOffset instant) => Date(CostaRicaTime.ToLocalDate(instant));

    public static string YesNo(bool value) => value ? "Sí" : "No";

    public static string OrEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? "(vacío)" : value;

    /// <summary>Recorta un texto al máximo permitido (por ejemplo, el detalle de auditoría).</summary>
    public static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";
}
