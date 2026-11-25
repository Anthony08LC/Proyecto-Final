using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Users.Commands.CreateUser;

/// <summary>
/// Crea un usuario (RF-USR-02). Solo Administrador. El correo no puede estar en uso, aunque sea
/// por un usuario inactivo (RN-30). La contraseña es temporal: el usuario deberá cambiarla
/// al iniciar sesión. Devuelve el id creado.
/// </summary>
public sealed record CreateUserCommand(
    string? FullName,
    string? Email,
    UserRole Role,
    string? TemporaryPassword) : ICommand<Guid>;
