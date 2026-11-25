using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Users.Commands.Login;

/// <summary>
/// Inicia sesión con correo y contraseña (RF-AUT-01). Con credenciales inválidas, usuario inactivo
/// o usuario de sistema responde siempre con el mismo mensaje genérico. Registra el intento,
/// exitoso o fallido. El límite de intentos por IP (RNF-SEG-05) se aplica en la API.
/// </summary>
public sealed record LoginCommand(string? Email, string? Password) : ICommand<LoginResult>;
