using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Application.Users.Common;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Users.Commands.UpdateProfile;

internal sealed class UpdateProfileCommandHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<UpdateProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> HandleAsync(
        UpdateProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var errors = new ValidationErrors();
        var fullName = errors.Required("fullName", "El nombre", command.FullName, User.FullNameMaxLength);
        errors.ThrowIfAny();

        var user = await users.GetRequiredAsync(currentUser.UserId, cancellationToken);
        var previousName = user.FullName;

        user.ChangeFullName(fullName);

        auditLog.Record(new AuditEntry(
            UserAuditEvents.Updated,
            AuditEntityType.User,
            user.Id,
            $"Perfil actualizado: nombre «{previousName}» → «{user.FullName}»."));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToProfileDto();
    }
}
