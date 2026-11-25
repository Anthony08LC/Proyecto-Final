namespace QuoteFlow.Application.Common.Exceptions;

/// <summary>
/// Se alcanzó un límite de uso, como las solicitudes por hora al asistente (RNF-SEG-12).
/// Se traduce a HTTP 429 con la cabecera <c>Retry-After</c>.
/// </summary>
public sealed class UsageLimitExceededException : Exception
{
    public UsageLimitExceededException(string message, DateTimeOffset retryAt)
        : base(message)
    {
        RetryAt = retryAt;
    }

    /// <summary>Momento a partir del cual se puede volver a usar.</summary>
    public DateTimeOffset RetryAt { get; }
}
