using QuoteFlow.Domain.Common;

namespace QuoteFlow.Application.Abstractions.Persistence;

/// <summary>
/// Repositorio base de un agregado. No expone borrado: los registros de negocio
/// no se eliminan físicamente (RN-20). Los cambios se confirman con <see cref="IUnitOfWork"/>.
/// </summary>
public interface IRepository<TEntity>
    where TEntity : AuditableEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(TEntity entity);
}
