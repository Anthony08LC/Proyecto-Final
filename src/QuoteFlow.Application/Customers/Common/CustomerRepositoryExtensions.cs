using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Customers.Common;

internal static class CustomerRepositoryExtensions
{
    /// <summary>Obtiene el cliente o lanza <see cref="NotFoundException"/> si no existe.</summary>
    public static async Task<Customer> GetRequiredAsync(
        this ICustomerRepository repository,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(customerId, cancellationToken)
            ?? throw new NotFoundException("el cliente", customerId);
    }
}
