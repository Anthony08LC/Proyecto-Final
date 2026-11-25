namespace QuoteFlow.Application.Common.Models;

/// <summary>Evento de auditoría ya registrado. Las fechas están en UTC.</summary>
public sealed record AuditEventRecord(
    long Id,
    DateTimeOffset OccurredAt,
    Guid? UserId,
    string? UserName,
    string EventType,
    AuditEntityType EntityType,
    Guid? EntityId,
    string Detail);
