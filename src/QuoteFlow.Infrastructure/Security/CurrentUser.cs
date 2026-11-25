using Microsoft.AspNetCore.Http;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Infrastructure.Security;

/// <summary>
/// Usuario autenticado de la solicitud HTTP actual, leído de los claims del token. Fuera de una
/// solicitud (por ejemplo, en el proceso de vencimiento) no hay usuario autenticado y los cambios
/// se atribuyen al usuario de sistema.
/// </summary>
internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true
        && UserId != Guid.Empty
        && Enum.IsDefined(Role);

    public Guid UserId =>
        Guid.TryParse(Claim(QuoteFlowClaims.UserId), out var id) ? id : Guid.Empty;

    public string FullName => Claim(QuoteFlowClaims.FullName) ?? string.Empty;

    public UserRole Role =>
        Enum.TryParse<UserRole>(Claim(QuoteFlowClaims.Role), ignoreCase: false, out var role)
            ? role
            : default;

    private string? Claim(string name) =>
        httpContextAccessor.HttpContext?.User is { } principal
            ? QuoteFlowClaims.Find(principal, name)
            : null;
}
