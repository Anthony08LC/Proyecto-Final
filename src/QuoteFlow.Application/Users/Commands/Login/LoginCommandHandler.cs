using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Users.Common;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Exceptions;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Application.Users.Commands.Login;

internal sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAccessTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    IAuditLog auditLog)
    : ICommandHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = TryParseEmail(command.Email);
        var user = email is null ? null : await users.GetByEmailAsync(email, cancellationToken);

        if (!CanSignIn(user, command.Password))
        {
            auditLog.Record(new AuditEntry(
                SessionAuditEvents.LoginFailed,
                AuditEntityType.Session,
                EntityId: null,
                $"Inicio de sesión fallido para «{email?.Value ?? "(correo no válido)"}».",
                ActorId: user is { IsSystem: false } ? user.Id : null,
                ActorName: user is { IsSystem: false } ? user.FullName : null));

            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new AuthenticationFailedException();
        }

        var token = tokenGenerator.Generate(user!);

        auditLog.Record(new AuditEntry(
            SessionAuditEvents.LoginSucceeded,
            AuditEntityType.Session,
            EntityId: null,
            $"Inicio de sesión de «{user!.FullName}».",
            ActorId: user.Id,
            ActorName: user.FullName));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResult(token.Token, token.ExpiresAt, user.ToProfileDto());
    }

    private bool CanSignIn(User? user, string? password) =>
        user is { IsActive: true, IsSystem: false, PasswordHash: not null }
        && !string.IsNullOrEmpty(password)
        && passwordHasher.Verify(password, user.PasswordHash);

    /// <summary>
    /// Normaliza el correo (minúsculas, sin espacios) para compararlo; un formato inválido
    /// simplemente no coincide con ningún usuario.
    /// </summary>
    private static Email? TryParseEmail(string? value)
    {
        try
        {
            return Email.CreateOptional(value);
        }
        catch (DomainException)
        {
            return null;
        }
    }
}
