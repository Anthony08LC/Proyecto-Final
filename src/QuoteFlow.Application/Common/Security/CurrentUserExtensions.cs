using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;

namespace QuoteFlow.Application.Common.Security;

public static class CurrentUserExtensions
{
    /// <summary>Exige que el usuario actual sea Administrador.</summary>
    public static void EnsureAdministrator(
        this ICurrentUser currentUser,
        string message = "Solo un Administrador puede realizar esta acción.")
    {
        if (!currentUser.IsAuthenticated || !currentUser.IsAdministrator)
        {
            throw new ForbiddenAccessException(message);
        }
    }

    /// <summary>Exige que haya un usuario autenticado.</summary>
    public static void EnsureAuthenticated(this ICurrentUser currentUser)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new ForbiddenAccessException("Debe iniciar sesión para realizar esta acción.");
        }
    }
}
