using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Domain.Common;

namespace QuoteFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio base de un agregado sobre EF Core. Los cambios se confirman con
/// <see cref="UnitOfWork"/>; no expone borrado (RN-20).
/// </summary>
internal abstract class Repository<TEntity>(ApplicationDbContext dbContext) : IRepository<TEntity>
    where TEntity : AuditableEntity
{
    protected ApplicationDbContext DbContext { get; } = dbContext;

    protected DbSet<TEntity> Set => DbContext.Set<TEntity>();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public void Add(TEntity entity) => Set.Add(entity);

    protected async Task<IReadOnlyList<TEntity>> FindByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken)
    {
        var distinct = ids.Distinct().ToList();

        if (distinct.Count == 0)
        {
            return [];
        }

        return await Set.Where(e => distinct.Contains(e.Id)).ToListAsync(cancellationToken);
    }
}
