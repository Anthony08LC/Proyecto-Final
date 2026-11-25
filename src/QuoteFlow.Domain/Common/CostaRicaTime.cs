namespace QuoteFlow.Domain.Common;

/// <summary>
/// Conversión a la hora de Costa Rica, usada por las reglas que dependen de la fecha
/// (RN-13 y RN-14). Costa Rica usa UTC−6 todo el año, sin horario de verano.
/// </summary>
public static class CostaRicaTime
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(-6);

    /// <summary>Devuelve la fecha calendario en Costa Rica para el instante indicado.</summary>
    public static DateOnly ToLocalDate(DateTimeOffset instant) =>
        DateOnly.FromDateTime(instant.ToOffset(Offset).DateTime);
}
