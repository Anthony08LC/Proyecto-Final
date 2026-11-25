using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Quotes.Commands.DuplicateQuote;

/// <summary>
/// Crea un borrador nuevo a partir de una cotización en cualquier estado (RF-COT-12). Ventas o
/// Administrador, sobre cualquier cotización. La copia lleva el cliente, las líneas y los textos,
/// toma los precios actuales y se recalcula con la política vigente (RN-23); su propietario es quien
/// duplica y guarda la referencia a la original (RN-24). Las líneas con productos inactivos se omiten
/// y se informan; si el cliente está inactivo, se rechaza (RN-25). La original no cambia.
/// </summary>
public sealed record DuplicateQuoteCommand(Guid QuoteId) : ICommand<DuplicateQuoteResult>;
