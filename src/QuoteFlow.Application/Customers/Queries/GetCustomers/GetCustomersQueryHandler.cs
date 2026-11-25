using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Customers.Common;

namespace QuoteFlow.Application.Customers.Queries.GetCustomers;

internal sealed class GetCustomersQueryHandler(
    ICustomerRepository customers,
    ICurrentUser currentUser)
    : IQueryHandler<GetCustomersQuery, PagedResult<CustomerSummaryDto>>
{
    public async Task<PagedResult<CustomerSummaryDto>> HandleAsync(
        GetCustomersQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var criteria = new CustomerSearchCriteria(
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            query.IsActive,
            query.IsVip);

        var page = await customers.SearchAsync(
            criteria,
            new PageRequest(query.Page, query.PageSize),
            cancellationToken);

        return new PagedResult<CustomerSummaryDto>(
            page.Items.Select(c => c.ToSummaryDto()).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}
