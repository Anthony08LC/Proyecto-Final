using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Customers.Common;

namespace QuoteFlow.Application.Customers.Queries.GetCustomerById;

internal sealed class GetCustomerByIdQueryHandler(
    ICustomerRepository customers,
    ICurrentUser currentUser)
    : IQueryHandler<GetCustomerByIdQuery, CustomerDto>
{
    public async Task<CustomerDto> HandleAsync(
        GetCustomerByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var customer = await customers.GetRequiredAsync(query.CustomerId, cancellationToken);
        return customer.ToDto();
    }
}
