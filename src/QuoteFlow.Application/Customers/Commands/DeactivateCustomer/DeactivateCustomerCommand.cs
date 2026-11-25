using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Customers.Commands.DeactivateCustomer;

/// <summary>Desactiva un cliente (RF-CLI-06). Solo Administrador. Su historial se conserva.</summary>
public sealed record DeactivateCustomerCommand(Guid CustomerId) : ICommand;
