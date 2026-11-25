using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Users.Commands.DeactivateUser;

/// <summary>
/// Desactiva un usuario (RF-USR-04). Solo Administrador. Nadie puede desactivarse a sí mismo
/// ni desactivar al último Administrador activo (RN-31). Se regenera el sello de seguridad,
/// por lo que sus sesiones dejan de funcionar de inmediato. Sus cotizaciones y eventos se conservan.
/// </summary>
public sealed record DeactivateUserCommand(Guid UserId) : ICommand;
