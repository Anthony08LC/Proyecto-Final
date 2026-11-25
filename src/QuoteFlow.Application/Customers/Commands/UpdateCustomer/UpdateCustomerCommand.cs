using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Customers.Common;

namespace QuoteFlow.Application.Customers.Commands.UpdateCustomer;

/// <summary>
/// Edita los datos de un cliente (RF-CLI-04). Solo Administrador.
/// La condición VIP se cambia con sus propios comandos (RF-CLI-05).
/// </summary>
public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string? Name,
    string? Company,
    string? Email,
    string? Phone,
    string? Notes) : ICommand<CustomerSaveResult>;
