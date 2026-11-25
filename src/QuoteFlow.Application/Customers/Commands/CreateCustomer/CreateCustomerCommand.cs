using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Customers.Common;

namespace QuoteFlow.Application.Customers.Commands.CreateCustomer;

/// <summary>Registra un cliente (RF-CLI-01). Solo un Administrador puede registrarlo como VIP (RN-21).</summary>
public sealed record CreateCustomerCommand(
    string? Name,
    string? Company,
    string? Email,
    string? Phone,
    string? Notes,
    bool IsVip = false) : ICommand<CustomerSaveResult>;
