using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Users.Commands.UpdateUser;

/// <summary>
/// Edita el nombre, el correo y el rol de un usuario (RF-USR-03). Solo Administrador.
/// Cambiar el rol regenera el sello de seguridad, y no se puede quitar el rol de Administrador
/// al último Administrador activo (RN-31).
/// </summary>
public sealed record UpdateUserCommand(
    Guid UserId,
    string? FullName,
    string? Email,
    UserRole Role) : ICommand;
