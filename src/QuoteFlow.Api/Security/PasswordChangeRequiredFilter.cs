using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using QuoteFlow.Infrastructure.Security;

namespace QuoteFlow.Api.Security;

/// <summary>
/// Cambio obligatorio de contraseña (RF-AUT-07): mientras el usuario tenga una contraseña temporal,
/// cualquier endpoint que no esté marcado con <see cref="AllowPendingPasswordChangeAttribute"/>
/// responde 403. El dato viene del token, que se invalida al cambiar la contraseña (nuevo sello).
/// </summary>
public sealed class PasswordChangeRequiredFilter(ProblemDetailsFactory problemDetailsFactory) : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        var endpointMetadata = context.ActionDescriptor.EndpointMetadata;

        if (endpointMetadata.OfType<IAllowAnonymous>().Any()
            || endpointMetadata.OfType<AllowPendingPasswordChangeAttribute>().Any())
        {
            return Task.CompletedTask;
        }

        var mustChange = QuoteFlowClaims.Find(user, QuoteFlowClaims.MustChangePassword);

        if (string.Equals(mustChange, "true", StringComparison.OrdinalIgnoreCase))
        {
            var problem = problemDetailsFactory.CreateProblemDetails(
                context.HttpContext,
                StatusCodes.Status403Forbidden,
                detail: "Debe cambiar su contraseña temporal antes de continuar.");

            problem.Extensions["code"] = "password_change_required";

            context.Result = new ObjectResult(problem) { StatusCode = StatusCodes.Status403Forbidden };
        }

        return Task.CompletedTask;
    }
}
