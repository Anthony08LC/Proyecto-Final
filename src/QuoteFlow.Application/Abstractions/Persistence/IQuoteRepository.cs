using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Abstractions.Persistence;

public interface IQuoteRepository : IRepository<Quote>
{
    /// <summary>
    /// Obtiene las cotizaciones emitidas cuya fecha de vencimiento es anterior a
    /// <paramref name="today"/> (fecha en hora de Costa Rica), para vencerlas (RF-COT-11).
    /// </summary>
    Task<IReadOnlyList<Quote>> GetIssuedExpiredBeforeAsync(
        DateOnly today,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca cotizaciones por número, cliente, estado, propietario y rango de fechas de creación.
    /// Orden: más recientes primero (RF-COT-14). No necesita cargar las líneas.
    /// </summary>
    Task<PagedResult<Quote>> SearchAsync(
        QuoteSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default);

    /// <summary>Cuenta las cotizaciones en el estado indicado (por ejemplo, borradores: RF-CFG-03).</summary>
    Task<int> CountByStatusAsync(QuoteStatus status, CancellationToken cancellationToken = default);
}

/// <summary>
/// Criterios de búsqueda de cotizaciones. Los valores nulos no filtran.
/// <paramref name="Number"/> se compara por coincidencia parcial sin distinguir mayúsculas.
/// <paramref name="CreatedFrom"/> es inclusivo y <paramref name="CreatedBefore"/> exclusivo.
/// </summary>
public sealed record QuoteSearchCriteria(
    string? Number,
    Guid? CustomerId,
    QuoteStatus? Status,
    Guid? OwnerId,
    DateTimeOffset? CreatedFrom,
    DateTimeOffset? CreatedBefore);
