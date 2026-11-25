using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Users.Common;

namespace QuoteFlow.Application.Users.Commands.UpdateProfile;

/// <summary>
/// El usuario autenticado cambia su nombre (RF-AUT-05). El correo y el rol solo los
/// modifica un Administrador (RF-USR-03).
/// </summary>
public sealed record UpdateProfileCommand(string? FullName) : ICommand<UserProfileDto>;
