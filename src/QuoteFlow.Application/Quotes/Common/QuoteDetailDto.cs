using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Quotes.Common;

/// <summary>Detalle completo de una cotización (RF-COT-13).</summary>
public sealed record QuoteDetailDto(
    Guid Id,
    string? Number,
    QuoteStatus Status,
    QuoteCustomerDto Customer,
    Guid OwnerId,
    string OwnerName,
    Guid? SourceQuoteId,
    string? SourceQuoteNumber,
    string? Introduction,
    string? ExecutiveSummary,
    string? Notes,
    string? CommercialTerms,
    IReadOnlyList<QuoteLineDto> Lines,
    QuoteBreakdownDto Breakdown,
    DateTimeOffset? IssuedAt,
    DateOnly? ValidUntil,
    DateTimeOffset? ClosedAt,
    string? CloseReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    QuoteActionsDto Actions,
    IReadOnlyList<QuoteHistoryEntryDto> History);
