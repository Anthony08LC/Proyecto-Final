using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Application.Quotes.Common;

internal static class QuoteMappings
{
    public static string DisplayNumber(this Quote quote) => quote.Number ?? "Borrador";

    public static QuoteBreakdownDto ToDto(this QuoteBreakdown breakdown, bool isEstimate) =>
        new(
            breakdown.Subtotal.Amount,
            breakdown.TierDiscountRate.Value,
            breakdown.VipDiscountRate.Value,
            breakdown.DiscountRate.Value,
            breakdown.DiscountAmount.Amount,
            breakdown.TaxableBase.Amount,
            breakdown.VatRate.Value,
            breakdown.VatAmount.Amount,
            breakdown.Total.Amount,
            isEstimate);

    public static QuoteBreakdownDto BreakdownDto(this Quote quote) =>
        quote.Breakdown.ToDto(quote.Status == QuoteStatus.Draft);

    public static QuoteLineDto ToDto(this QuoteLine line, bool? productIsActive) =>
        new(
            line.LineNumber,
            line.ProductId,
            line.ProductName,
            line.ProductDescription,
            line.CategoryName,
            line.UnitPrice.Amount,
            line.Quantity,
            line.CalculateSubtotal().Amount,
            productIsActive);
}
