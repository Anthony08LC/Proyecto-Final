namespace QuoteFlow.Application.Common.Exceptions;

/// <summary>Causa por la que el asistente no pudo responder (sección 11.4).</summary>
public enum AssistantFailure
{
    /// <summary>No hay clave de Gemini configurada. HTTP 503.</summary>
    NotConfigured = 1,

    /// <summary>Error del servicio o respuesta inválida. HTTP 502.</summary>
    ServiceError = 2,

    /// <summary>Se excedió el tiempo límite. HTTP 504.</summary>
    Timeout = 3
}

/// <summary>
/// El asistente de IA no pudo generar la sugerencia. El borrador no cambia y el resto del sistema
/// sigue funcionando (sección 11.4). El mensaje es apto para mostrarse al usuario.
/// </summary>
public sealed class AssistantUnavailableException : Exception
{
    public AssistantUnavailableException(AssistantFailure failure, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Failure = failure;
    }

    public AssistantFailure Failure { get; }

    public static AssistantUnavailableException NotConfigured() => new(
        AssistantFailure.NotConfigured,
        "El asistente de redacción no está disponible: no se configuró la conexión con Gemini. "
        + "Puede escribir los textos manualmente.");

    public static AssistantUnavailableException ServiceError(Exception? innerException = null) => new(
        AssistantFailure.ServiceError,
        "El asistente de redacción no pudo generar una sugerencia en este momento. "
        + "El borrador no cambió; intente de nuevo más tarde.",
        innerException);

    public static AssistantUnavailableException Timeout(Exception? innerException = null) => new(
        AssistantFailure.Timeout,
        "El asistente de redacción tardó demasiado en responder. El borrador no cambió; puede reintentar.",
        innerException);
}
