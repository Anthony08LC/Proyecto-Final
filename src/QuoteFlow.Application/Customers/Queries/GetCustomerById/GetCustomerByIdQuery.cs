using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Customers.Common;

namespace QuoteFlow.Application.Customers.Queries.GetCustomerById;

/// <summary>Obtiene el detalle de un cliente (RF-CLI-03).</summary>
public sealed record GetCustomerByIdQuery(Guid CustomerId) : IQuery<CustomerDto>;
