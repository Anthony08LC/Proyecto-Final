using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Users.Common;

internal static class UserExtensions
{
    public static UserDto ToDto(this User user) =>
        new(
            user.Id,
            user.FullName,
            user.Email.Value,
            user.Role,
            user.IsActive,
            user.MustChangePassword,
            user.CreatedAt,
            user.UpdatedAt);

    public static UserProfileDto ToProfileDto(this User user) =>
        new(user.Id, user.FullName, user.Email.Value, user.Role, user.MustChangePassword);

    /// <summary>
    /// Obtiene un usuario gestionable. El usuario de sistema no se expone ni se modifica,
    /// por lo que se trata como inexistente.
    /// </summary>
    public static async Task<User> GetRequiredAsync(
        this IUserRepository repository,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(userId, cancellationToken);

        return user is null || user.IsSystem
            ? throw new NotFoundException("el usuario", userId)
            : user;
    }

    /// <summary>
    /// Garantiza que siempre exista al menos un Administrador activo (RN-31) antes de que
    /// <paramref name="user"/> deje de serlo (por desactivación o cambio de rol).
    /// </summary>
    public static async Task EnsureNotLastActiveAdministratorAsync(
        this IUserRepository repository,
        User user,
        string message,
        CancellationToken cancellationToken)
    {
        if (user is not { Role: UserRole.Administrator, IsActive: true })
        {
            return;
        }

        if (await repository.CountActiveAdministratorsAsync(cancellationToken) <= 1)
        {
            throw new ConflictException(message);
        }
    }
}
