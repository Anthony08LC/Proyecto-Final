using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Infrastructure.Persistence;

namespace QuoteFlow.Infrastructure.Auditing;

/// <summary>Lectura del historial de auditoría (solo lectura, sin seguimiento de cambios).</summary>
internal sealed class AuditEventReader(ApplicationDbContext dbContext) : IAuditEventReader
{
    public async Task<IReadOnlyList<AuditEventRecord>> GetByEntityAsync(
        AuditEntityType entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var type = entityType.ToString();

        var events = await dbContext.AuditEvents
            .AsNoTracking()
            .Where(e => e.EntityType == type && e.EntityId == entityId)
            .OrderBy(e => e.OccurredAt)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);

        return events
            .Select(e => new AuditEventRecord(
                e.Id,
                e.OccurredAt,
                e.UserId,
                e.UserName,
                e.EventType,
                Enum.Parse<AuditEntityType>(e.EntityType),
                e.EntityId,
                e.Detail))
            .ToList();
    }
}
