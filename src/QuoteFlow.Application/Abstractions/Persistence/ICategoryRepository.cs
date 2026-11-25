using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Abstractions.Persistence;

public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Indica si existe otra categoría con el mismo nombre, sin distinguir mayúsculas
    /// y aunque esté inactiva (RN-30).
    /// </summary>
    Task<bool> NameExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la categoría con el nombre indicado, sin distinguir mayúsculas y aunque esté
    /// inactiva. Permite sugerir la reactivación de una categoría existente (RF-CAT-01).
    /// </summary>
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista las categorías filtradas por nombre y estado, ordenadas por nombre (RF-CAT-02).
    /// </summary>
    Task<PagedResult<Category>> SearchAsync(
        CategorySearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default);
}

/// <summary>Criterios de búsqueda de categorías. Los valores nulos no filtran.</summary>
public sealed record CategorySearchCriteria(string? Search, bool? IsActive);
