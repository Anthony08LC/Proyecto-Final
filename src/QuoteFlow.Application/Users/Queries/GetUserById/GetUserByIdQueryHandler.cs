using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Users.Common;

namespace QuoteFlow.Application.Users.Queries.GetUserById;

internal sealed class GetUserByIdQueryHandler(
    IUserRepository users,
    ICurrentUser currentUser)
    : IQueryHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var user = await users.GetRequiredAsync(query.UserId, cancellationToken);
        return user.ToDto();
    }
}
