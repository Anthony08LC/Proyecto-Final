namespace QuoteFlow.Application.Quotes.Commands.IssueQuote;

/// <summary>Resultado de la emisión: número asignado, fecha de emisión (UTC) y vencimiento.</summary>
public sealed record IssueQuoteResult(string Number, DateTimeOffset IssuedAt, DateOnly ValidUntil);
