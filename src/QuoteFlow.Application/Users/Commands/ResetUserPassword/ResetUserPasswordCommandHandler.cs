using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Application.Users.Common;

namespace QuoteFlow.Application.Users.Commands.ResetUserPassword;

internal sealed class ResetUserPasswordCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<ResetUserPasswordCommand>
{
    public async Task HandleAsync(
        ResetUserPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var errors = new ValidationErrors();
        PasswordPolicy.Validate(errors, "temporaryPassword", command.TemporaryPassword);
        errors.ThrowIfAny();

        var user = await users.GetRequiredAsync(command.UserId, cancellationToken);

        user.ChangePassword(passwordHasher.Hash(command.TemporaryPassword!), isTemporary: true);

        auditLog.Record(new AuditEntry(
            UserAuditEvents.PasswordReset,
            AuditEntityType.User,
            user.Id,
            $"Contraseña del usuario «{user.FullName}» restablecida."));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
