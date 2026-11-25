using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application.Quotes.Commands.AddQuoteLine;

/// <summary>
/// Agrega un producto activo a un borrador (RF-COT-02). Propietario o Administrador. Si el producto
/// ya está en la cotización, suma la cantidad (RN-28). La línea toma el precio y los datos actuales
/// del catálogo y el desglose se recalcula. Devuelve el desglose actualizado.
/// </summary>
public sealed record AddQuoteLineCommand(Guid QuoteId, Guid ProductId, decimal Quantity)
    : ICommand<QuoteBreakdownDto>;
