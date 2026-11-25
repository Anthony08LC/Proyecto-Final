namespace QuoteFlow.Application.Common.Models;

/// <summary>Token de sesión emitido al autenticarse.</summary>
public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);
