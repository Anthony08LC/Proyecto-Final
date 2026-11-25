using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Users.Commands.ResetUserPassword;

/// <summary>
/// Asigna una contraseña temporal a un usuario (RF-USR-05). Solo Administrador. El usuario
/// deberá cambiarla al iniciar sesión y sus sesiones actuales se invalidan.
/// La contraseña nunca aparece en la auditoría.
/// </summary>
public sealed record ResetUserPasswordCommand(Guid UserId, string? TemporaryPassword) : ICommand;
