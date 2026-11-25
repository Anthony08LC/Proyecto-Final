using QuoteFlow.Application.Common.Models;

namespace QuoteFlow.Application.Abstractions.Services;

/// <summary>Lectura del historial de eventos de auditoría (solo lectura).</summary>
public interface IAuditEventReader
{
    /// <summary>
    /// Obtiene los eventos de una entidad, del más antiguo al más reciente
    /// (por ejemplo, el historial de una cotización: RF-COT-13).
    /// </summary>
    Task<IReadOnlyList<AuditEventRecord>> GetByEntityAsync(
        AuditEntityType entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Momentos, del más antiguo al más reciente, en que el usuario generó eventos del tipo indicado
    /// desde <paramref name="since"/> (por ejemplo, el límite de uso del asistente: RNF-SEG-12).
    /// </summary>
    Task<IReadOnlyList<DateTimeOffset>> GetOccurrencesByUserAsync(
        Guid userId,
        string eventType,
        DateTimeOffset since,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Últimos <paramref name="count"/> eventos, del más reciente al más antiguo; con
    /// <paramref name="userId"/>, solo los de ese usuario (actividad reciente: RF-DSH-03).
    /// </summary>
    Task<IReadOnlyList<AuditEventRecord>> GetRecentAsync(
        Guid? userId,
        int count,
        CancellationToken cancellationToken = default);
}
