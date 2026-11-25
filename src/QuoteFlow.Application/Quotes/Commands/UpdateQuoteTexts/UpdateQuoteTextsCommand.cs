using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Quotes.Commands.UpdateQuoteTexts;

/// <summary>
/// Edita los textos comerciales de un borrador: introducción, resumen ejecutivo, observaciones y
/// condiciones comerciales (RF-COT-05). Propietario o Administrador. Los cuatro son opcionales,
/// con un máximo de 4000 caracteres cada uno. También se usa para aceptar una sugerencia de la IA.
/// </summary>
public sealed record UpdateQuoteTextsCommand(
    Guid QuoteId,
    string? Introduction,
    string? ExecutiveSummary,
    string? Notes,
    string? CommercialTerms) : ICommand;
