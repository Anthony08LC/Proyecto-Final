using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Domain.Common;

namespace QuoteFlow.Application.Common.Persistence;

internal static class RepositoryExtensions
{
    /// <summary>Obtiene la entidad o lanza <see cref="NotFoundException"/> si no existe.</summary>
    public static async Task<TEntity> GetRequiredAsync<TEntity>(
        this IRepository<TEntity> repository,
        Guid id,
        string entityName,
        CancellationToken cancellationToken)
        where TEntity : AuditableEntity
    {
        return await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(entityName, id);
    }
}
