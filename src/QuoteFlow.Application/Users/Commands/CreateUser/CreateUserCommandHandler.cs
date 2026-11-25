using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Application.Users.Common;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Users.Commands.CreateUser;

internal sealed class CreateUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var errors = new ValidationErrors();
        var input = UserInput.Parse(errors, command.FullName, command.Email, command.Role);
        PasswordPolicy.Validate(errors, "temporaryPassword", command.TemporaryPassword);
        errors.ThrowIfAny();

        if (await users.EmailExistsAsync(input.Email, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"El correo «{input.Email.Value}» ya está en uso por otro usuario.");
        }

        var user = User.Create(
            input.FullName,
            input.Email,
            passwordHasher.Hash(command.TemporaryPassword!),
            input.Role);

        users.Add(user);

        auditLog.Record(new AuditEntry(
            UserAuditEvents.Created,
            AuditEntityType.User,
            user.Id,
            $"Usuario «{user.FullName}» ({user.Email.Value}) creado con rol {user.Role}."));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
