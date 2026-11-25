namespace QuoteFlow.Infrastructure.Assistant;

/// <summary>
/// Configuración de Google Gemini (sección 11). Se lee de las variables de entorno
/// <c>Gemini__ApiKey</c> y <c>Gemini__Model</c>. La clave es opcional: sin ella el asistente queda
/// «no disponible» y el resto del sistema funciona igual (RF-IA-03).
/// </summary>
public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public const string DefaultModel = "gemini-2.5-flash";

    public const string DefaultEndpoint = "https://generativelanguage.googleapis.com/v1beta/";

    /// <summary>Clave de la API; nunca se registra en los logs.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Modelo a usar, por ejemplo <c>gemini-2.5-flash</c>.</summary>
    public string? Model { get; set; }

    /// <summary>URL base de la API (configurable para pruebas).</summary>
    public string Endpoint { get; set; } = DefaultEndpoint;

    /// <summary>Tiempo límite de cada llamada (RNF-REN-04): 20 segundos.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>Máximo de tokens de salida (incluye el razonamiento interno de los modelos que lo usan).</summary>
    public int MaxOutputTokens { get; set; } = 4096;

    public double Temperature { get; set; } = 0.7;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);

    public string ResolvedModel => string.IsNullOrWhiteSpace(Model) ? DefaultModel : Model.Trim();
}
