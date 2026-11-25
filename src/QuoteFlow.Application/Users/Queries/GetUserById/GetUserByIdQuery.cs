using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Users.Common;

namespace QuoteFlow.Application.Users.Queries.GetUserById;

/// <summary>Obtiene el detalle de un usuario. Solo Administrador.</summary>
public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;
