using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Auditing.Common;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Domain.Common;

namespace QuoteFlow.Application.Auditing.Queries.GetAuditEvents;

internal sealed class GetAuditEventsQueryHandler(
    IAuditEventReader auditEvents,
    ICurrentUser currentUser)
    : IQueryHandler<GetAuditEventsQuery, PagedResult<AuditEventDto>>
{
    public async Task<PagedResult<AuditEventDto>> HandleAsync(
        GetAuditEventsQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        if (query.From is { } from && query.To is { } to && from > to)
        {
            throw new ValidationException("from", "La fecha inicial no puede ser posterior a la fecha final.");
        }

        if (query.EntityType is { } entityType && !Enum.IsDefined(entityType))
        {
            throw new ValidationException("entityType", "El tipo de entidad indicado no es válido.");
        }

        var criteria = new AuditEventSearchCriteria(
            query.UserId,
            string.IsNullOrWhiteSpace(query.EventType) ? null : query.EventType.Trim(),
            query.EntityType,
            query.From is { } start ? StartOfDay(start) : null,
            query.To is { } end ? StartOfDay(end.AddDays(1)) : null);

        var page = await auditEvents.SearchAsync(
            criteria,
            new PageRequest(query.Page, query.PageSize),
            cancellationToken);

        var items = page.Items
            .Select(e => new AuditEventDto(
                e.Id, e.OccurredAt, e.UserId, e.UserName, e.EventType, e.EntityType, e.EntityId, e.Detail))
            .ToList();

        return new PagedResult<AuditEventDto>(items, page.Page, page.PageSize, page.TotalCount);
    }

    /// <summary>Inicio del día indicado en hora de Costa Rica (RNF-DAT-03).</summary>
    private static DateTimeOffset StartOfDay(DateOnly date) =>
        new(date.ToDateTime(TimeOnly.MinValue), CostaRicaTime.Offset);
}
