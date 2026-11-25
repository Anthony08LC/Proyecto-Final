using QuoteFlow.Application.Assistant;

namespace QuoteFlow.Application.Abstractions.Services;

/// <summary>
/// Asistente de redacción comercial (sección 11). Hay una implementación real con Google Gemini y
/// otra «no disponible» que se usa cuando no hay clave configurada (RF-IA-03): la aplicación funciona
/// igual sin él.
/// </summary>
public interface ITextAssistant
{
    /// <summary>Falso si el asistente no está configurado; sus funciones se deshabilitan.</summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Devuelve el texto propuesto por el modelo, sin procesar. Los errores del servicio, las
    /// respuestas inválidas y el tiempo límite excedido se informan con
    /// <see cref="Common.Exceptions.AssistantUnavailableException"/>.
    /// </summary>
    Task<string> CompleteAsync(AssistantPrompt prompt, CancellationToken cancellationToken = default);
}
