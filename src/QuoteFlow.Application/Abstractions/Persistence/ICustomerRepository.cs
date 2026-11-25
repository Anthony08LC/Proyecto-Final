using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Application.Abstractions.Persistence;

public interface ICustomerRepository : IRepository<Customer>
{
    /// <summary>
    /// Indica si otro cliente ya usa el correo. No es una restricción de unicidad: sirve para
    /// advertir de un posible duplicado (RF-CLI-01).
    /// </summary>
    Task<bool> EmailInUseAsync(
        Email email,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca clientes por nombre, empresa o correo sin distinguir mayúsculas, con filtros
    /// por estado y condición VIP. El resultado se ordena por nombre (RF-CLI-02).
    /// </summary>
    Task<PagedResult<Customer>> SearchAsync(
        CustomerSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene varios clientes a la vez (por ejemplo, para listar cotizaciones).</summary>
    Task<IReadOnlyList<Customer>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default);

    /// <summary>Cuenta los clientes activos (dashboard: RF-DSH-01).</summary>
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
}

/// <summary>Criterios de búsqueda de clientes. Los valores nulos no filtran.</summary>
public sealed record CustomerSearchCriteria(string? Search, bool? IsActive, bool? IsVip);
