using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Documents.Queries.GetQuotePreview;

/// <summary>
/// Vista previa HTML del documento comercial (RF-DOC-01). Disponible en cualquier estado para
/// Ventas y Administrador; los borradores llevan marca de agua (RN-36). Devuelve la página HTML.
/// </summary>
public sealed record GetQuotePreviewQuery(Guid QuoteId) : IQuery<string>;
