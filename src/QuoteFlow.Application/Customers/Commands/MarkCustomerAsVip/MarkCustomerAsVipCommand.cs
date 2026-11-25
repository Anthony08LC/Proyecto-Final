using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Customers.Commands.MarkCustomerAsVip;

/// <summary>Marca un cliente como VIP (RF-CLI-05). Solo Administrador (RN-21).</summary>
public sealed record MarkCustomerAsVipCommand(Guid CustomerId) : ICommand;
