using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Application.Users.Common;

namespace QuoteFlow.Application.Users.Commands.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAccessTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<ChangePasswordCommand, AccessToken>
{
    public async Task<AccessToken> HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var user = await users.GetRequiredAsync(currentUser.UserId, cancellationToken);

        var errors = new ValidationErrors();

        if (string.IsNullOrEmpty(command.CurrentPassword)
            || user.PasswordHash is null
            || !passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
        {
            errors.Add("currentPassword", "La contraseña actual no es correcta.");
        }

        PasswordPolicy.Validate(errors, "newPassword", command.NewPassword);

        if (!errors.HasErrors && command.NewPassword == command.CurrentPassword)
        {
            errors.Add("newPassword", "La nueva contraseña debe ser distinta de la actual.");
        }

        errors.ThrowIfAny();

        user.ChangePassword(passwordHasher.Hash(command.NewPassword!), isTemporary: false);

        auditLog.Record(new AuditEntry(
            UserAuditEvents.PasswordChanged,
            AuditEntityType.User,
            user.Id,
            $"El usuario «{user.FullName}» cambió su contraseña."));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return tokenGenerator.Generate(user);
    }
}
