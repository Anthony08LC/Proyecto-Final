using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Customers.Commands.ReactivateCustomer;

/// <summary>Reactiva un cliente inactivo (RF-CLI-06). Solo Administrador.</summary>
public sealed record ReactivateCustomerCommand(Guid CustomerId) : ICommand;
