using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Api.ErrorHandling;

/// <summary>
/// Manejo global de excepciones (RNF-MAN-05): traduce cada excepción conocida a una respuesta
/// <i>ProblemDetails</i> con el código HTTP correspondiente. Los errores inesperados se registran
/// y se responden con un mensaje genérico, sin trazas, SQL ni rutas (RNF-SEG-10).
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            // El cliente canceló la solicitud: no hay a quién responder.
            httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            return true;
        }

        var problem = CreateProblem(exception);

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Error no controlado al procesar {Method} {Path}.",
                httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("Solicitud rechazada ({StatusCode}): {Message}", problem.Status, exception.Message);
        }

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private static ProblemDetails CreateProblem(Exception exception) => exception switch
    {
        ValidationException validation => new HttpValidationProblemDetails(
            validation.Errors.ToDictionary(e => e.Key, e => e.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Datos no válidos",
            Detail = validation.Message
        },
        DomainException => Problem(
            StatusCodes.Status422UnprocessableEntity,
            "Regla de negocio incumplida",
            exception.Message),
        NotFoundException => Problem(StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message),
        ConflictException => Problem(StatusCodes.Status409Conflict, "Conflicto", exception.Message),
        ForbiddenAccessException => Problem(StatusCodes.Status403Forbidden, "Acceso denegado", exception.Message),
        AuthenticationFailedException => Problem(
            StatusCodes.Status401Unauthorized,
            "No autenticado",
            exception.Message),
        BadHttpRequestException badRequest => Problem(
            badRequest.StatusCode,
            "Solicitud no válida",
            "La solicitud no tiene un formato válido."),
        _ => Problem(
            StatusCodes.Status500InternalServerError,
            "Error interno",
            "Ocurrió un error inesperado. Intente de nuevo más tarde.")
    };

    private static ProblemDetails Problem(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };
}
