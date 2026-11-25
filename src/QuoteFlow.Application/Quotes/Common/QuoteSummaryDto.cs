using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Quotes.Common;

/// <summary>Cotización en listados y búsquedas (RF-COT-14).</summary>
public sealed record QuoteSummaryDto(
    Guid Id,
    string? Number,
    QuoteStatus Status,
    Guid CustomerId,
    string CustomerName,
    Guid OwnerId,
    string OwnerName,
    decimal Total,
    bool IsEstimate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? IssuedAt,
    DateOnly? ValidUntil);
