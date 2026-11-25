using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Common.Models;

namespace QuoteFlow.Application.Users.Commands.ChangePassword;

/// <summary>
/// El usuario autenticado cambia su propia contraseña (RF-AUT-06, RF-AUT-07). Exige la contraseña
/// actual; la nueva cumple la política y es distinta. Se regenera el sello de seguridad (las demás
/// sesiones quedan invalidadas) y se devuelve un token nuevo para la sesión actual.
/// </summary>
public sealed record ChangePasswordCommand(string? CurrentPassword, string? NewPassword) : ICommand<AccessToken>;
