using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;

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
}
