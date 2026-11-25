using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Infrastructure.Persistence;
using QuoteFlow.Infrastructure.Persistence.Records;

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

        return events.Select(ToRecord).ToList();
    }

    public async Task<IReadOnlyList<DateTimeOffset>> GetOccurrencesByUserAsync(
        Guid userId,
        string eventType,
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditEvents
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.OccurredAt >= since && e.EventType == eventType)
            .OrderBy(e => e.OccurredAt)
            .Select(e => e.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditEventRecord>> GetRecentAsync(
        Guid? userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AuditEvents.AsNoTracking();

        if (userId is { } id)
        {
            query = query.Where(e => e.UserId == id);
        }

        var events = await query
            .OrderByDescending(e => e.OccurredAt)
            .ThenByDescending(e => e.Id)
            .Take(count)
            .ToListAsync(cancellationToken);

        return events.Select(ToRecord).ToList();
    }

    private static AuditEventRecord ToRecord(AuditEvent e) =>
        new(
            e.Id,
            e.OccurredAt,
            e.UserId,
            e.UserName,
            e.EventType,
            Enum.Parse<AuditEntityType>(e.EntityType),
            e.EntityId,
            e.Detail);
}
