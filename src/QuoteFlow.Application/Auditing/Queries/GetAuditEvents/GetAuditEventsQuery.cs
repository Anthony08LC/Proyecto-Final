using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Auditing.Common;
using QuoteFlow.Application.Common.Models;

namespace QuoteFlow.Application.Auditing.Queries.GetAuditEvents;

/// <summary>
/// Consulta el historial de auditoría con filtros por usuario, tipo de evento, tipo de entidad y
/// rango de fechas (en hora de Costa Rica, ambos extremos inclusivos). Más recientes primero y
/// paginado. Solo lectura y solo Administrador (RF-AUD-02).
/// </summary>
public sealed record GetAuditEventsQuery(
    Guid? UserId = null,
    string? EventType = null,
    AuditEntityType? EntityType = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize) : IQuery<PagedResult<AuditEventDto>>;
