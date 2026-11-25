using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application.Quotes.Commands.RemoveQuoteLine;

/// <summary>
/// Quita una línea de un borrador (RF-COT-03); las restantes se renumeran. Propietario o
/// Administrador. Es el único borrado físico permitido (RN-20). Devuelve el desglose recalculado.
/// </summary>
public sealed record RemoveQuoteLineCommand(Guid QuoteId, Guid ProductId) : ICommand<QuoteBreakdownDto>;
