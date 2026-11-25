using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Application.Abstractions.Persistence;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>Indica si otro usuario ya usa el correo, aunque esté inactivo (RN-30).</summary>
    Task<bool> EmailExistsAsync(
        Email email,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Cuenta los Administradores activos, para garantizar que siempre exista uno (RN-31).</summary>
    Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca usuarios por nombre o correo sin distinguir mayúsculas, con filtros por rol y estado.
    /// Nunca incluye al usuario de sistema (RF-USR-01). Ordenado por nombre.
    /// </summary>
    Task<PagedResult<User>> SearchAsync(
        UserSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default);

    /// <summary>Obtiene varios usuarios a la vez (por ejemplo, los propietarios de cotizaciones).</summary>
    Task<IReadOnlyList<User>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default);
}

/// <summary>Criterios de búsqueda de usuarios. Los valores nulos no filtran.</summary>
public sealed record UserSearchCriteria(string? Search, UserRole? Role, bool? IsActive);
