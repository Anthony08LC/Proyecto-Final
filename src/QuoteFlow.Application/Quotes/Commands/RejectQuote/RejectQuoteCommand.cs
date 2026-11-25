using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Quotes.Commands.RejectQuote;

/// <summary>
/// Registra que el cliente rechazó la cotización (RF-COT-09). Propietario o Administrador.
/// Solo desde Emitida; el motivo es opcional (máximo 500 caracteres).
/// </summary>
public sealed record RejectQuoteCommand(Guid QuoteId, string? Reason) : ICommand;
