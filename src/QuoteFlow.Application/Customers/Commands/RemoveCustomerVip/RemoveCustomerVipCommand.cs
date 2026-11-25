using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Customers.Commands.RemoveCustomerVip;

/// <summary>Quita la condición VIP de un cliente (RF-CLI-05). Solo Administrador (RN-21).</summary>
public sealed record RemoveCustomerVipCommand(Guid CustomerId) : ICommand;
