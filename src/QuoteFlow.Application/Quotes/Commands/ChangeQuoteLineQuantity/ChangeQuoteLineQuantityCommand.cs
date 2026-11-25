using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application.Quotes.Commands.ChangeQuoteLineQuantity;

/// <summary>
/// Modifica la cantidad de una línea de un borrador (RF-COT-03). Propietario o Administrador.
/// La cantidad se valida igual que al agregar (RN-07). Devuelve el desglose recalculado.
/// </summary>
public sealed record ChangeQuoteLineQuantityCommand(Guid QuoteId, Guid ProductId, decimal Quantity)
    : ICommand<QuoteBreakdownDto>;
