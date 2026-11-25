using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Abstractions.Persistence;

public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Indica si existe otro producto con el mismo nombre, sin distinguir mayúsculas
    /// y aunque esté inactivo (RN-30).
    /// </summary>
    Task<bool> NameExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene los productos de las líneas de una cotización (recálculo y emisión).</summary>
    Task<IReadOnlyList<Product>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca productos por nombre sin distinguir mayúsculas, con filtros por categoría y estado.
    /// El resultado se ordena por nombre (RF-PRO-02).
    /// </summary>
    Task<PagedResult<Product>> SearchAsync(
        ProductSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuenta los productos (activos e inactivos) de cada categoría indicada (RF-CAT-02).
    /// Las categorías sin productos pueden no aparecer en el resultado.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> CountByCategoryAsync(
        IEnumerable<Guid> categoryIds,
        CancellationToken cancellationToken = default);

    /// <summary>Cuenta los productos activos (dashboard: RF-DSH-01).</summary>
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
}

/// <summary>Criterios de búsqueda de productos. Los valores nulos no filtran.</summary>
public sealed record ProductSearchCriteria(string? Search, Guid? CategoryId, bool? IsActive);
