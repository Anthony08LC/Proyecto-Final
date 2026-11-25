using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(ApplicationDbContext dbContext)
    : Repository<Category>(dbContext), ICategoryRepository
{
    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();

        return Set.AnyAsync(
            c => c.Name.ToLower() == normalized && (excludingId == null || c.Id != excludingId),
            cancellationToken);
    }

    public Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        return Set.FirstOrDefaultAsync(c => c.Name.ToLower() == normalized, cancellationToken);
    }

    public Task<IReadOnlyList<Category>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default) =>
        FindByIdsAsync(ids, cancellationToken);

    public Task<PagedResult<Category>> SearchAsync(
        CategorySearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = Set;

        if (criteria.Search is { } search)
        {
            var pattern = LikePattern.Contains(search);
            query = query.Where(c => EF.Functions.ILike(c.Name, pattern));
        }

        if (criteria.IsActive is { } isActive)
        {
            query = query.Where(c => c.IsActive == isActive);
        }

        return query
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .ToPagedResultAsync(page, cancellationToken);
    }
}
