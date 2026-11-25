using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Users.Common;

namespace QuoteFlow.Application.Users.Queries.GetCurrentUserProfile;

/// <summary>Obtiene el perfil del usuario autenticado: nombre, correo y rol (RF-AUT-05).</summary>
public sealed record GetCurrentUserProfileQuery : IQuery<UserProfileDto>;
