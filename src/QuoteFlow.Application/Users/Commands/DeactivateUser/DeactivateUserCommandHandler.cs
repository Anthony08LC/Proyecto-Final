using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Users.Common;

namespace QuoteFlow.Application.Users.Commands.DeactivateUser;

internal sealed class DeactivateUserCommandHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<DeactivateUserCommand>
{
    public async Task HandleAsync(
        DeactivateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        if (command.UserId == currentUser.UserId)
        {
            throw new ConflictException("No puede desactivar su propia cuenta.");
        }

        var user = await users.GetRequiredAsync(command.UserId, cancellationToken);

        if (!user.IsActive)
        {
            throw new ConflictException("El usuario ya está inactivo.");
        }

        await users.EnsureNotLastActiveAdministratorAsync(
            user,
            "No se puede desactivar al último Administrador activo.",
            cancellationToken);

        user.Deactivate();

        auditLog.Record(new AuditEntry(
            UserAuditEvents.Deactivated,
            AuditEntityType.User,
            user.Id,
            $"Usuario «{user.FullName}» ({user.Email.Value}) desactivado."));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
