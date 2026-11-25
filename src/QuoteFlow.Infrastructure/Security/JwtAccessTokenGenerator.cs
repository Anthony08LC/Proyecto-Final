using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Infrastructure.Security;

/// <summary>
/// Emite el JWT de sesión firmado con HMAC-SHA256: 8 horas de vida, con el identificador, el rol
/// y el sello de seguridad del usuario (sección 10.1).
/// </summary>
internal sealed class JwtAccessTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider)
    : IAccessTokenGenerator
{
    private readonly JsonWebTokenHandler _handler = new();

    public AccessToken Generate(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var settings = options.Value;

        if (!settings.HasValidSigningKey())
        {
            throw new InvalidOperationException(
                $"La clave de firma del JWT no está configurada o tiene menos de {JwtOptions.MinSigningKeyBytes} bytes.");
        }

        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddHours(settings.LifetimeHours);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(
            [
                new Claim(QuoteFlowClaims.UserId, user.Id.ToString()),
                new Claim(QuoteFlowClaims.FullName, user.FullName),
                new Claim(QuoteFlowClaims.Email, user.Email.Value),
                new Claim(QuoteFlowClaims.Role, user.Role.ToString()),
                new Claim(QuoteFlowClaims.SecurityStamp, user.SecurityStamp.ToString()),
                new Claim(
                    QuoteFlowClaims.MustChangePassword,
                    user.MustChangePassword.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(),
                    ClaimValueTypes.Boolean)
            ])
        };

        return new AccessToken(_handler.CreateToken(descriptor), expiresAt);
    }
}
