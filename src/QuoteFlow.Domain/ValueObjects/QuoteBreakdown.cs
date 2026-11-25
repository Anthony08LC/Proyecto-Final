using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Domain.ValueObjects;

/// <summary>
/// Desglose financiero de una cotización producido por el motor de cálculo.
/// Inmutable y coherente por construcción: verifica las relaciones aritméticas
/// entre sus montos y porcentajes.
/// </summary>
public sealed record QuoteBreakdown
{
    public static readonly QuoteBreakdown Empty = new(
        Money.Zero,
        Percentage.Zero,
        Percentage.Zero,
        Money.Zero,
        Percentage.Zero,
        Money.Zero);

    public Money Subtotal { get; }

    /// <summary>Porcentaje de descuento por tramo (RN-01, RN-02).</summary>
    public Percentage TierDiscountRate { get; }

    /// <summary>Porcentaje de descuento VIP (RN-03).</summary>
    public Percentage VipDiscountRate { get; }

    /// <summary>Porcentaje total de descuento: tramo + VIP.</summary>
    public Percentage DiscountRate { get; }

    public Money DiscountAmount { get; }

    /// <summary>Base imponible: subtotal − monto de descuento.</summary>
    public Money TaxableBase { get; }

    public Percentage VatRate { get; }

    public Money VatAmount { get; }

    /// <summary>Total: base imponible + monto de IVA.</summary>
    public Money Total { get; }

    internal QuoteBreakdown(
        Money subtotal,
        Percentage tierDiscountRate,
        Percentage vipDiscountRate,
        Money discountAmount,
        Percentage vatRate,
        Money vatAmount)
    {
        if (subtotal is null || tierDiscountRate is null || vipDiscountRate is null
            || discountAmount is null || vatRate is null || vatAmount is null)
        {
            throw new DomainException("Todos los valores del desglose son obligatorios.");
        }

        if (discountAmount > subtotal)
        {
            throw new DomainException("El monto de descuento no puede superar el subtotal.");
        }

        Subtotal = subtotal;
        TierDiscountRate = tierDiscountRate;
        VipDiscountRate = vipDiscountRate;
        DiscountRate = tierDiscountRate + vipDiscountRate;
        DiscountAmount = discountAmount;
        TaxableBase = subtotal - discountAmount;
        VatRate = vatRate;
        VatAmount = vatAmount;
        Total = TaxableBase + VatAmount;
    }
}
