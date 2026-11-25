namespace QuoteFlow.Application.Quotes.Common;

/// <summary>Evento del historial de una cotización. La fecha está en UTC.</summary>
public sealed record QuoteHistoryEntryDto(
    DateTimeOffset OccurredAt,
    string? UserName,
    string EventType,
    string Detail);
