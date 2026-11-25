using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Users.Common;

namespace QuoteFlow.Application.Users.Queries.GetUsers;

internal sealed class GetUsersQueryHandler(
    IUserRepository users,
    ICurrentUser currentUser)
    : IQueryHandler<GetUsersQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var criteria = new UserSearchCriteria(
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            query.Role,
            query.IsActive);

        var page = await users.SearchAsync(
            criteria,
            new PageRequest(query.Page, query.PageSize),
            cancellationToken);

        return new PagedResult<UserDto>(
            page.Items.Select(u => u.ToDto()).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}
