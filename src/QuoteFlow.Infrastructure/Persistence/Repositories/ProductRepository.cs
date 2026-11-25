using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(ApplicationDbContext dbContext)
    : Repository<Product>(dbContext), IProductRepository
{
    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();

        return Set.AnyAsync(
            p => p.Name.ToLower() == normalized && (excludingId == null || p.Id != excludingId),
            cancellationToken);
    }

    public Task<IReadOnlyList<Product>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default) =>
        FindByIdsAsync(ids, cancellationToken);

    public Task<PagedResult<Product>> SearchAsync(
        ProductSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = Set;

        if (criteria.Search is { } search)
        {
            var pattern = LikePattern.Contains(search);
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }

        if (criteria.CategoryId is { } categoryId)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (criteria.IsActive is { } isActive)
        {
            query = query.Where(p => p.IsActive == isActive);
        }

        return query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .ToPagedResultAsync(page, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> CountByCategoryAsync(
        IEnumerable<Guid> categoryIds,
        CancellationToken cancellationToken = default)
    {
        var ids = categoryIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        return await Set
            .Where(p => ids.Contains(p.CategoryId))
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);
    }
}
