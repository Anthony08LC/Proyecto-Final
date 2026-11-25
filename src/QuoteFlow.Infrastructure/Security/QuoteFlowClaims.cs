using System.Security.Claims;

namespace QuoteFlow.Infrastructure.Security;

/// <summary>
/// Claims del token de sesión (RNF-SEG-04). La API los usa al validar cada solicitud, por ejemplo
/// para comparar el sello de seguridad con el de la base de datos (RF-AUT-03).
/// </summary>
public static class QuoteFlowClaims
{
    public const string UserId = "sub";
    public const string FullName = "name";
    public const string Email = "email";
    public const string Role = "role";
    public const string SecurityStamp = "security_stamp";
    public const string MustChangePassword = "must_change_password";

    /// <summary>Lee un claim por su nombre corto o por el equivalente de .NET si se mapearon.</summary>
    public static string? Find(ClaimsPrincipal principal, string shortName) =>
        principal.FindFirst(shortName)?.Value
        ?? shortName switch
        {
            UserId => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            FullName => principal.FindFirst(ClaimTypes.Name)?.Value,
            Email => principal.FindFirst(ClaimTypes.Email)?.Value,
            Role => principal.FindFirst(ClaimTypes.Role)?.Value,
            _ => null
        };
}
