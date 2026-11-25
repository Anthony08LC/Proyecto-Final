using System.Text;

namespace QuoteFlow.Infrastructure.Security;

/// <summary>
/// Configuración del token de sesión (sección <c>Jwt</c>). La clave de firma proviene de variables
/// de entorno (por ejemplo, <c>Jwt__SigningKey</c>) y nunca del repositorio (RNF-SEG-07).
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Longitud mínima de la clave para HMAC-SHA256: 32 bytes.</summary>
    public const int MinSigningKeyBytes = 32;

    public string Issuer { get; set; } = "QuoteFlow";

    public string Audience { get; set; } = "QuoteFlow";

    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Duración de la sesión: 8 horas, sin refresh tokens (RNF-SEG-04).</summary>
    public int LifetimeHours { get; set; } = 8;

    public bool HasValidSigningKey() => Encoding.UTF8.GetByteCount(SigningKey) >= MinSigningKeyBytes;
}
