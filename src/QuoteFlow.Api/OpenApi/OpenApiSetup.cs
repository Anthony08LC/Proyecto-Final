using Microsoft.OpenApi;

namespace QuoteFlow.Api.OpenApi;

public static class OpenApiSetup
{
    public const string DocumentUrl = "/openapi/v1.json";
    private const string BearerScheme = "Bearer";

    /// <summary>
    /// Documento OpenAPI con el esquema de seguridad Bearer (JWT), para probar los endpoints
    /// desde Swagger UI con el token obtenido en el inicio de sesión.
    /// </summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "QuoteFlow AI API",
                Version = "v1",
                Description = "API de cotizaciones de TechSolutions CR. Inicie sesión en POST /api/auth/login "
                    + "y use el token en el botón «Authorize»."
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Token JWT obtenido en POST /api/auth/login."
            };

            document.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(BearerScheme, document)] = []
                }
            ];

            return Task.CompletedTask;
        }));

        return services;
    }

    /// <summary>Publica el documento OpenAPI y Swagger UI en <c>/swagger</c>.</summary>
    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi().AllowAnonymous();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(DocumentUrl, "QuoteFlow AI v1");
            options.DocumentTitle = "QuoteFlow AI API";
        });

        return app;
    }
}
