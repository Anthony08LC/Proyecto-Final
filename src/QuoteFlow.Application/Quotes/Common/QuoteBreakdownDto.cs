namespace QuoteFlow.Application.Quotes.Common;

/// <summary>
/// Desglose financiero calculado por el motor (RF-COT-06). En borradores los montos son
/// estimados (<paramref name="IsEstimate"/>) y la interfaz los rotula como tales.
/// </summary>
public sealed record QuoteBreakdownDto(
    decimal Subtotal,
    decimal TierDiscountRate,
    decimal VipDiscountRate,
    decimal DiscountRate,
    decimal DiscountAmount,
    decimal TaxableBase,
    decimal VatRate,
    decimal VatAmount,
    decimal Total,
    bool IsEstimate);
