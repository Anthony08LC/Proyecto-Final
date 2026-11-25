using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Users.Common;

namespace QuoteFlow.Application.Users.Queries.GetCurrentUserProfile;

internal sealed class GetCurrentUserProfileQueryHandler(
    IUserRepository users,
    ICurrentUser currentUser)
    : IQueryHandler<GetCurrentUserProfileQuery, UserProfileDto>
{
    public async Task<UserProfileDto> HandleAsync(
        GetCurrentUserProfileQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var user = await users.GetRequiredAsync(currentUser.UserId, cancellationToken);
        return user.ToProfileDto();
    }
}
