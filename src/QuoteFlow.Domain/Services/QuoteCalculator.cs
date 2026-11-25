using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Exceptions;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Domain.Services;

/// <summary>
/// Motor de cálculo oficial de las cotizaciones (sección 7.1 de la especificación):
/// <list type="number">
/// <item>Total de línea = redondear(cantidad × precio unitario).</item>
/// <item>Subtotal = suma de los totales de línea.</item>
/// <item>% por tramo según RN-01 y RN-02.</item>
/// <item>% VIP según RN-03.</item>
/// <item>% total de descuento = % por tramo + % VIP.</item>
/// <item>Monto de descuento = redondear(subtotal × % total ÷ 100).</item>
/// <item>Base imponible = subtotal − monto de descuento.</item>
/// <item>Monto de IVA = redondear(base imponible × % IVA ÷ 100).</item>
/// <item>Total = base imponible + monto de IVA.</item>
/// </list>
/// Todo redondeo es a 2 decimales con los valores medios alejándose de cero (RN-17),
/// y lo encapsula <see cref="Money"/>. El cálculo es puro: no modifica sus entradas.
/// </summary>
public static class QuoteCalculator
{
    /// <summary>Calcula el desglose de la cotización con el cliente y la configuración vigentes.</summary>
    public static QuoteBreakdown Calculate(Quote quote, Customer customer, SystemSettings settings)
    {
        if (quote is null)
        {
            throw new DomainException("La cotización es obligatoria.");
        }

        if (customer is null)
        {
            throw new DomainException("El cliente es obligatorio.");
        }

        if (customer.Id != quote.CustomerId)
        {
            throw new DomainException("El cliente no corresponde a la cotización.");
        }

        return Calculate(quote.Lines, customer.IsVip, settings);
    }

    /// <summary>Calcula el desglose a partir de las líneas y la condición VIP del cliente.</summary>
    public static QuoteBreakdown Calculate(
        IEnumerable<QuoteLine> lines,
        bool customerIsVip,
        SystemSettings settings)
    {
        if (lines is null)
        {
            throw new DomainException("Las líneas de la cotización son obligatorias.");
        }

        EnsureValidPolicy(settings);

        var subtotal = CalculateSubtotal(lines);
        var tierRate = CalculateTierDiscountRate(subtotal, settings);
        var vipRate = CalculateVipDiscountRate(customerIsVip, settings);
        var discountRate = CalculateDiscountRate(tierRate, vipRate);
        var discountAmount = subtotal.PercentageOf(discountRate);
        var taxableBase = subtotal - discountAmount;
        var vatAmount = taxableBase.PercentageOf(settings.VatPercentage);

        return new QuoteBreakdown(
            subtotal,
            tierRate,
            vipRate,
            discountAmount,
            settings.VatPercentage,
            vatAmount);
    }

    /// <summary>Paso 1 y 2: suma de los totales de línea, cada uno redondeado (RN-17).</summary>
    public static Money CalculateSubtotal(IEnumerable<QuoteLine> lines) =>
        lines.Aggregate(Money.Zero, (sum, line) => sum + line.CalculateSubtotal());

    /// <summary>
    /// Paso 3 (RN-01, RN-02): tramo 2 si el subtotal es mayor que su umbral; tramo 1 si es
    /// mayor que el umbral 1 y menor o igual que el umbral 2; 0 % en otro caso.
    /// Los tramos son excluyentes.
    /// </summary>
    public static Percentage CalculateTierDiscountRate(Money subtotal, SystemSettings settings)
    {
        if (subtotal > settings.Tier2Threshold)
        {
            return settings.Tier2Discount;
        }

        return subtotal > settings.Tier1Threshold ? settings.Tier1Discount : Percentage.Zero;
    }

    /// <summary>Paso 4 (RN-03): se suma el % VIP si el cliente es VIP, haya o no tramo.</summary>
    public static Percentage CalculateVipDiscountRate(bool customerIsVip, SystemSettings settings) =>
        customerIsVip ? settings.VipDiscount : Percentage.Zero;

    /// <summary>
    /// Paso 5: % total = % por tramo + % VIP. RN-27 garantiza que no supera 100 %;
    /// <see cref="Percentage"/> lo vuelve a verificar.
    /// </summary>
    public static Percentage CalculateDiscountRate(Percentage tierRate, Percentage vipRate) =>
        tierRate + vipRate;

    /// <summary>
    /// Verifica que la configuración cumpla RN-27 antes de calcular, para que ningún
    /// desglose se produzca con una política comercial inválida.
    /// </summary>
    private static void EnsureValidPolicy(SystemSettings? settings)
    {
        if (settings is null)
        {
            throw new DomainException("La configuración del sistema es obligatoria.");
        }

        settings.EnsureValidCommercialPolicy();
    }
}
