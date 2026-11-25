using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Customers.Common;

internal static class CustomerMappings
{
    public static CustomerDto ToDto(this Customer customer) =>
        new(
            customer.Id,
            customer.Name,
            customer.Company,
            customer.Email?.Value,
            customer.Phone?.Value,
            customer.IsVip,
            customer.IsActive,
            customer.Notes,
            customer.CreatedAt,
            customer.UpdatedAt);

    public static CustomerSummaryDto ToSummaryDto(this Customer customer) =>
        new(
            customer.Id,
            customer.Name,
            customer.Company,
            customer.Email?.Value,
            customer.Phone?.Value,
            customer.IsVip,
            customer.IsActive);
}
