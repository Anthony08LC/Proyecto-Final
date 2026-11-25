using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Quotes.Commands.CancelQuote;

/// <summary>
/// Cancela un borrador o una cotización emitida (RF-COT-10). Propietario o Administrador.
/// Si fue emitida, el motivo es obligatorio (RN-34). Un borrador cancelado no tiene número.
/// </summary>
public sealed record CancelQuoteCommand(Guid QuoteId, string? Reason) : ICommand;
