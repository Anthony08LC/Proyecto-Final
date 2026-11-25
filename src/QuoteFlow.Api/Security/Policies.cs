using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Api.Security;

/// <summary>
/// Políticas de autorización por rol (sección 10.2). Todo endpoint exige autenticación salvo el
/// inicio de sesión (RF-AUT-04); las verificaciones de propiedad (RN-32) las hacen los casos de uso.
/// </summary>
public static class Policies
{
    /// <summary>Solo Administradores: usuarios, configuración y gestión del catálogo y de clientes.</summary>
    public const string Administrator = nameof(Administrator);

    public static readonly string AdministratorRole = nameof(UserRole.Administrator);
}
