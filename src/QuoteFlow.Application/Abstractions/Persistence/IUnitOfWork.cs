namespace QuoteFlow.Application.Abstractions.Persistence;

/// <summary>
/// Confirma en una sola transacción los cambios de los repositorios y los eventos de auditoría.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Descarta los cambios en memoria de las entidades cargadas, para que no se guarden en el
    /// siguiente <see cref="SaveChangesAsync"/>. Los eventos de auditoría pendientes se conservan.
    /// Lo usa el asistente, que recalcula el borrador solo para armar el contexto (sección 11.2).
    /// </summary>
    void DiscardChanges();

    /// <summary>
    /// Ejecuta una operación dentro de una transacción explícita; si falla, se revierte completa.
    /// Necesaria cuando hay bloqueos de fila, como en la emisión de cotizaciones.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
