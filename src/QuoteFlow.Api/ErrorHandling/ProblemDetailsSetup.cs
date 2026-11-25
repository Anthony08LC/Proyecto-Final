using System.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;

namespace QuoteFlow.Api.ErrorHandling;

public static class ProblemDetailsSetup
{
    private static readonly Dictionary<int, string> SpanishTitles = new()
    {
        [StatusCodes.Status400BadRequest] = "Solicitud no válida",
        [StatusCodes.Status401Unauthorized] = "No autenticado",
        [StatusCodes.Status403Forbidden] = "Acceso denegado",
        [StatusCodes.Status404NotFound] = "Recurso no encontrado",
        [StatusCodes.Status405MethodNotAllowed] = "Método no permitido",
        [StatusCodes.Status409Conflict] = "Conflicto",
        [StatusCodes.Status415UnsupportedMediaType] = "Tipo de contenido no admitido",
        [StatusCodes.Status422UnprocessableEntity] = "Regla de negocio incumplida",
        [StatusCodes.Status429TooManyRequests] = "Demasiadas solicitudes",
        [StatusCodes.Status500InternalServerError] = "Error interno",
        [StatusCodes.Status503ServiceUnavailable] = "Servicio no disponible"
    };

    private static readonly Dictionary<int, string> DefaultDetails = new()
    {
        [StatusCodes.Status401Unauthorized] = "Debe iniciar sesión o su sesión ya no es válida.",
        [StatusCodes.Status403Forbidden] = "No tiene permiso para realizar esta acción.",
        [StatusCodes.Status404NotFound] = "El recurso solicitado no existe.",
        [StatusCodes.Status429TooManyRequests] = "Se superó el límite de intentos. Espere un momento e intente de nuevo."
    };

    /// <summary>
    /// Respuestas de error uniformes en formato <i>ProblemDetails</i> (RFC 9457), en español y con
    /// el identificador de traza para correlacionarlas con los logs. Incluye las respuestas sin
    /// cuerpo de la autenticación (401/403) y las generadas por el enrutamiento.
    /// </summary>
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var problem = context.ProblemDetails;
            var status = problem.Status ?? context.HttpContext.Response.StatusCode;

            if (SpanishTitles.TryGetValue(status, out var title)
                && (string.IsNullOrEmpty(problem.Title) || IsDefaultEnglishTitle(problem.Title, status)))
            {
                problem.Title = title;
            }

            if (string.IsNullOrEmpty(problem.Detail) && DefaultDetails.TryGetValue(status, out var detail))
            {
                problem.Detail = detail;
            }

            problem.Instance ??= $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            problem.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    /// <summary>
    /// Los títulos por defecto de ASP.NET Core están en inglés (la frase del código HTTP o el
    /// título genérico de validación); se reemplazan por los títulos en español.
    /// </summary>
    private static bool IsDefaultEnglishTitle(string title, int status) =>
        title == ReasonPhrases.GetReasonPhrase(status)
        || title == "One or more validation errors occurred."
        || title == "An error occurred while processing your request.";
}
