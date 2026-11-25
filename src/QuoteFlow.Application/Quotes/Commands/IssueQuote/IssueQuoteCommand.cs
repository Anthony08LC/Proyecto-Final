using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Quotes.Commands.IssueQuote;

/// <summary>
/// Emite un borrador (RF-COT-07). Propietario o Administrador. Se rechaza sin líneas (RN-09) o con
/// el cliente o algún producto inactivo (RN-16), con la causa exacta. Recalcula por última vez y
/// congela los datos (RN-12), asigna el consecutivo del año de emisión en hora de Costa Rica (RN-13)
/// y la fecha de vencimiento (RN-14). Todo ocurre en una transacción: si falla, no se consume número
/// y la cotización sigue siendo un borrador. Un conflicto de concurrencia se informa como
/// <see cref="Application.Common.Exceptions.ConflictException"/> desde la persistencia.
/// </summary>
public sealed record IssueQuoteCommand(Guid QuoteId) : ICommand<IssueQuoteResult>;
