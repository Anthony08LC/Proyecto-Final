namespace QuoteFlow.Application.Quotes.Common;

/// <summary>
/// Línea de una cotización. <paramref name="ProductIsActive"/> solo se informa en borradores,
/// para advertir que un producto inactivo bloquea la emisión (RN-16).
/// </summary>
public sealed record QuoteLineDto(
    int LineNumber,
    Guid ProductId,
    string ProductName,
    string? ProductDescription,
    string CategoryName,
    decimal UnitPrice,
    decimal Quantity,
    decimal LineTotal,
    bool? ProductIsActive);
