using QuoteFlow.Application.Users.Common;

namespace QuoteFlow.Application.Users.Commands.Login;

/// <summary>
/// Sesión iniciada: token JWT de 8 horas y perfil del usuario. Si
/// <see cref="UserProfileDto.MustChangePassword"/> es verdadero, solo podrá cambiar su contraseña (RF-AUT-07).
/// </summary>
public sealed record LoginResult(string Token, DateTimeOffset ExpiresAt, UserProfileDto User);
