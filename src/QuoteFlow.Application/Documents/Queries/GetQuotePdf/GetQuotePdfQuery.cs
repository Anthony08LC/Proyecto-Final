using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Documents.Queries.GetQuotePdf;

/// <summary>
/// PDF del documento comercial (RF-DOC-02). Solo para cotizaciones emitidas, es decir, con número
/// (RN-36); las que ya no están Emitidas muestran su estado. El archivo se llama
/// <c>COT-AAAA-000001.pdf</c>.
/// </summary>
public sealed record GetQuotePdfQuery(Guid QuoteId) : IQuery<QuotePdfFile>;
