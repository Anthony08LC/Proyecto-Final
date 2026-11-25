using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.Services;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de cotizaciones. Las líneas se cargan siempre con la cotización (navegación con
/// inclusión automática), salvo en los listados, que no las necesitan.
/// </summary>
internal sealed class QuoteRepository(ApplicationDbContext dbContext)
    : Repository<Quote>(dbContext), IQuoteRepository
{
    public async Task<IReadOnlyList<Quote>> GetIssuedExpiredBeforeAsync(
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        return await Set
            .Where(q => q.Status == QuoteStatus.Issued && q.ValidUntil < today)
            .OrderBy(q => q.ValidUntil)
            .ToListAsync(cancellationToken);
    }

    public Task<PagedResult<Quote>> SearchAsync(
        QuoteSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Quote> query = Set.IgnoreAutoIncludes();

        if (criteria.Number is { } number)
        {
            var pattern = LikePattern.Contains(number);
            query = query.Where(q => q.Number != null && EF.Functions.ILike(q.Number, pattern));
        }

        if (criteria.CustomerId is { } customerId)
        {
            query = query.Where(q => q.CustomerId == customerId);
        }

        if (criteria.Status is { } status)
        {
            query = query.Where(q => q.Status == status);
        }

        if (criteria.OwnerId is { } ownerId)
        {
            query = query.Where(q => q.OwnerId == ownerId);
        }

        if (criteria.CreatedFrom is { } from)
        {
            var fromUtc = from.ToUniversalTime();
            query = query.Where(q => q.CreatedAt >= fromUtc);
        }

        if (criteria.CreatedBefore is { } before)
        {
            var beforeUtc = before.ToUniversalTime();
            query = query.Where(q => q.CreatedAt < beforeUtc);
        }

        return query
            .OrderByDescending(q => q.CreatedAt)
            .ThenByDescending(q => q.Id)
            .ToPagedResultAsync(page, cancellationToken);
    }

    public Task<int> CountByStatusAsync(QuoteStatus status, CancellationToken cancellationToken = default) =>
        Set.IgnoreAutoIncludes().CountAsync(q => q.Status == status, cancellationToken);

    public async Task<IReadOnlyList<QuoteStatusTotals>> GetTotalsByStatusAsync(
        CancellationToken cancellationToken = default)
    {
        // El total es un value object con conversión: la agregación se hace directamente en SQL.
        var rows = await DbContext.Database
            .SqlQuery<StatusTotalsRow>($"""
                SELECT status AS status,
                       COUNT(*)::int AS count,
                       COUNT(number)::int AS numbered_count,
                       COALESCE(SUM(total), 0) AS total
                FROM quotes
                GROUP BY status
                """)
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new QuoteStatusTotals(
                Enum.Parse<QuoteStatus>(r.Status),
                r.Count,
                r.NumberedCount,
                Money.Create(r.Total)))
            .ToList();
    }

    /// <summary>Fila de la agregación por estado; los alias usan snake_case porque la convención de nombres también se aplica a SqlQuery.</summary>
    private sealed class StatusTotalsRow
    {
        public string Status { get; init; } = string.Empty;

        public int Count { get; init; }

        public int NumberedCount { get; init; }

        public decimal Total { get; init; }
    }
}
