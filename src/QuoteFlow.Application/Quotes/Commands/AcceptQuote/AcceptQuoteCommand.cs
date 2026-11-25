using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Quotes.Commands.AcceptQuote;

/// <summary>
/// Registra que el cliente aceptó la cotización (RF-COT-08). Propietario o Administrador. Solo desde
/// Emitida y dentro de la vigencia (RN-35), aunque el proceso diario aún no la haya vencido.
/// </summary>
public sealed record AcceptQuoteCommand(Guid QuoteId) : ICommand;
