using System.Globalization;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Domain.Common;

namespace QuoteFlow.Application.Assistant;

/// <summary>
/// Límite de uso del asistente: 30 solicitudes por hora por usuario, en una ventana móvil (RNF-SEG-12).
/// Se cuenta sobre los eventos de auditoría de sugerencias generadas, así que el límite se mantiene
/// aunque la API se reinicie o corra en varias instancias.
/// </summary>
internal sealed class AssistantUsageLimiter(IAuditEventReader auditEvents, TimeProvider timeProvider)
{
    public const int RequestsPerHour = 30;

    public static readonly TimeSpan Window = TimeSpan.FromHours(1);

    /// <summary>Solicitudes que le quedan al usuario en la hora actual.</summary>
    public async Task<int> GetRemainingAsync(Guid userId, CancellationToken cancellationToken)
    {
        var recent = await GetRecentAsync(userId, cancellationToken);
        return Math.Max(0, RequestsPerHour - recent.Count);
    }

    /// <summary>Lanza <see cref="UsageLimitExceededException"/> si el usuario ya alcanzó el límite.</summary>
    public async Task EnsureWithinLimitAsync(Guid userId, CancellationToken cancellationToken)
    {
        var recent = await GetRecentAsync(userId, cancellationToken);
        if (recent.Count < RequestsPerHour)
        {
            return;
        }

        // Se libera un cupo cuando la solicitud más antigua que mantiene lleno el límite sale de la ventana.
        var retryAt = recent[recent.Count - RequestsPerHour] + Window;
        var localTime = retryAt.ToOffset(CostaRicaTime.Offset).ToString("HH:mm", CultureInfo.InvariantCulture);

        throw new UsageLimitExceededException(
            $"Alcanzó el límite de {RequestsPerHour} solicitudes por hora al asistente de redacción. "
            + $"Podrá volver a usarlo a partir de las {localTime}.",
            retryAt);
    }

    private Task<IReadOnlyList<DateTimeOffset>> GetRecentAsync(Guid userId, CancellationToken cancellationToken) =>
        auditEvents.GetOccurrencesByUserAsync(
            userId,
            Quotes.Common.QuoteAuditEvents.TextGenerated,
            timeProvider.GetUtcNow() - Window,
            cancellationToken);
}
