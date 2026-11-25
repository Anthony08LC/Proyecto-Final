using QuoteFlow.Application.Common.Models;

namespace QuoteFlow.Application.Auditing.Common;

/// <summary>
/// Evento de auditoría para su consulta (RF-AUD-02). La fecha está en UTC; la interfaz la muestra
/// en hora de Costa Rica. <c>UserName</c> es la copia del nombre al momento del evento.
/// </summary>
public sealed record AuditEventDto(
    long Id,
    DateTimeOffset OccurredAt,
    Guid? UserId,
    string? UserName,
    string EventType,
    AuditEntityType EntityType,
    Guid? EntityId,
    string Detail);
