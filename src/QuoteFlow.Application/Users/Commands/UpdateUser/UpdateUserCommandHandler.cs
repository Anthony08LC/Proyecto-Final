using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Application.Users.Common;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<UpdateUserCommand>
{
    public async Task HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var user = await users.GetRequiredAsync(command.UserId, cancellationToken);

        var errors = new ValidationErrors();
        var input = UserInput.Parse(errors, command.FullName, command.Email, command.Role);
        errors.ThrowIfAny();

        if (await users.EmailExistsAsync(input.Email, user.Id, cancellationToken))
        {
            throw new ConflictException($"El correo «{input.Email.Value}» ya está en uso por otro usuario.");
        }

        var previousRole = user.Role;
        var roleChanged = previousRole != input.Role;

        if (roleChanged && previousRole == UserRole.Administrator)
        {
            await users.EnsureNotLastActiveAdministratorAsync(
                user,
                "No se puede quitar el rol de Administrador al último Administrador activo.",
                cancellationToken);
        }

        user.ChangeFullName(input.FullName);
        user.ChangeEmail(input.Email);

        auditLog.Record(new AuditEntry(
            UserAuditEvents.Updated,
            AuditEntityType.User,
            user.Id,
            $"Datos del usuario «{user.FullName}» ({user.Email.Value}) actualizados."));

        if (roleChanged)
        {
            user.ChangeRole(input.Role);

            auditLog.Record(new AuditEntry(
                UserAuditEvents.RoleChanged,
                AuditEntityType.User,
                user.Id,
                $"Rol del usuario «{user.FullName}»: {previousRole} → {user.Role}."));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
