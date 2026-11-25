using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Users.Common;

namespace QuoteFlow.Application.Users.Commands.ReactivateUser;

internal sealed class ReactivateUserCommandHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<ReactivateUserCommand>
{
    public async Task HandleAsync(
        ReactivateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var user = await users.GetRequiredAsync(command.UserId, cancellationToken);

        if (user.IsActive)
        {
            throw new ConflictException("El usuario ya está activo.");
        }

        user.Activate();

        auditLog.Record(new AuditEntry(
            UserAuditEvents.Reactivated,
            AuditEntityType.User,
            user.Id,
            $"Usuario «{user.FullName}» ({user.Email.Value}) reactivado."));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
