using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Quotes.Common;

/// <summary>
/// Regla de propiedad (RN-32): Ventas ve todas las cotizaciones, pero solo modifica y cambia
/// el estado de las suyas; el Administrador puede hacerlo con todas.
/// </summary>
internal static class QuoteAccess
{
    public static bool CanManage(this ICurrentUser currentUser, Quote quote) =>
        currentUser.IsAuthenticated
        && (currentUser.IsAdministrator || quote.OwnerId == currentUser.UserId);

    public static void EnsureCanManage(this ICurrentUser currentUser, Quote quote)
    {
        currentUser.EnsureAuthenticated();

        if (!currentUser.CanManage(quote))
        {
            throw new ForbiddenAccessException(
                "Solo el propietario de la cotización o un Administrador puede modificarla o cambiar su estado.");
        }
    }
}
