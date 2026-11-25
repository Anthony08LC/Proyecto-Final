namespace QuoteFlow.Application.Abstractions.Persistence;

/// <summary>
/// Confirma en una sola transacción los cambios de los repositorios y los eventos de auditoría.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta una operación dentro de una transacción explícita; si falla, se revierte completa.
    /// Necesaria cuando hay bloqueos de fila, como en la emisión de cotizaciones.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
