namespace QuoteFlow.Application.Quotes.Common;

/// <summary>Tipos de evento de auditoría de las cotizaciones.</summary>
internal static class QuoteAuditEvents
{
    public const string Created = "QuoteCreated";
    public const string Duplicated = "QuoteDuplicated";
    public const string Issued = "QuoteIssued";
    public const string Accepted = "QuoteAccepted";
    public const string Rejected = "QuoteRejected";
    public const string Cancelled = "QuoteCancelled";
    public const string Expired = "QuoteExpired";

    /// <summary>Sugerencia del asistente de IA generada; el detalle nunca incluye el texto (RNF-SEG-12).</summary>
    public const string TextGenerated = "QuoteTextGenerated";
}
