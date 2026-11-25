using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Users.Commands.ReactivateUser;

/// <summary>Reactiva un usuario inactivo (RF-USR-04). Solo Administrador.</summary>
public sealed record ReactivateUserCommand(Guid UserId) : ICommand;
