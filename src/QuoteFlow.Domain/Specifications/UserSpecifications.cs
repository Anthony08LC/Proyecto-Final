using System.Linq.Expressions;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Domain.Specifications;

/// <summary>Criterios de usuario reutilizables, traducibles a SQL por la persistencia.</summary>
public static class UserSpecifications
{
    /// <summary>
    /// Usuarios que cuentan para la regla del último Administrador activo (RN-31): rol Administrador,
    /// activos y que no sean el usuario de sistema. El usuario de sistema tiene rol Administrador,
    /// pero no inicia sesión, así que no puede administrar el sistema y no debe contar.
    /// </summary>
    public static readonly Expression<Func<User, bool>> CountsAsActiveAdministrator =
        u => u.Role == UserRole.Administrator && u.IsActive && !u.IsSystem;
}
