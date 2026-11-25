using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Infrastructure.Security;

namespace QuoteFlow.Api.Security;

public static class AuthenticationSetup
{
    /// <summary>
    /// Autenticación JWT (sección 10.1) y autorización por rol (sección 10.2). La política por
    /// defecto exige usuario autenticado en todos los endpoints; solo el inicio de sesión es anónimo.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        // Sin clave de firma válida la API no puede emitir ni validar sesiones: se detiene al arrancar.
        services.AddOptions<JwtOptions>()
            .Validate(
                options => options.HasValidSigningKey(),
                $"Configure la clave de firma del JWT (variable de entorno Jwt__SigningKey) con al menos {JwtOptions.MinSigningKeyBytes} bytes.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = QuoteFlowClaims.FullName,
                    RoleClaimType = QuoteFlowClaims.Role
                };
                bearer.Events = new JwtBearerEvents { OnTokenValidated = ValidateSecurityStampAsync };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(Policies.Administrator, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(Policies.AdministratorRole));

        return services;
    }

    /// <summary>
    /// Valida la sesión en cada solicitud (RF-AUT-03): el usuario debe seguir activo y el sello de
    /// seguridad del token debe coincidir con el de la base de datos. Así quedan invalidadas de
    /// inmediato las sesiones de usuarios desactivados o con rol o contraseña cambiados.
    /// </summary>
    private static async Task ValidateSecurityStampAsync(TokenValidatedContext context)
    {
        var principal = context.Principal;
        var userIdClaim = principal is null ? null : QuoteFlowClaims.Find(principal, QuoteFlowClaims.UserId);
        var stampClaim = principal is null ? null : QuoteFlowClaims.Find(principal, QuoteFlowClaims.SecurityStamp);

        if (!Guid.TryParse(userIdClaim, out var userId) || !Guid.TryParse(stampClaim, out var stamp))
        {
            context.Fail("El token no contiene los datos de sesión requeridos.");
            return;
        }

        var users = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await users.GetByIdAsync(userId, context.HttpContext.RequestAborted);

        if (user is null || !user.IsActive || user.IsSystem || user.SecurityStamp != stamp)
        {
            context.Fail("La sesión ya no es válida.");
        }
    }
}
