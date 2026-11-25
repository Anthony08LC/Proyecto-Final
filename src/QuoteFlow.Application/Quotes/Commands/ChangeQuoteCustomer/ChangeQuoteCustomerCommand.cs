using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application.Quotes.Commands.ChangeQuoteCustomer;

/// <summary>
/// Reasigna el cliente de un borrador (RF-COT-04). Propietario o Administrador. El nuevo cliente
/// debe estar activo y el descuento VIP se recalcula según él. Devuelve el desglose recalculado.
/// </summary>
public sealed record ChangeQuoteCustomerCommand(Guid QuoteId, Guid CustomerId) : ICommand<QuoteBreakdownDto>;
