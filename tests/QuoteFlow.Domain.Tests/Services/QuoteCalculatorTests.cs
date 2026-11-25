using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Domain.Tests.Services;

/// <summary>
/// Casos críticos del motor de cálculo (plan.md, sección 9.4; RF-COT-06; RNF-MAN-04).
/// Configuración inicial: tramo 1 &gt; ₡500.000 → 5 %; tramo 2 &gt; ₡1.000.000 → 10 %; VIP 3 %;
/// IVA 13 %. Redondeo a 2 decimales con los valores medios alejándose de cero (RN-17).
/// </summary>
public sealed class QuoteCalculatorTests
{
    [Fact(DisplayName = "C-01 · Borrador vacío: todos los montos en ₡0,00")]
    public void C01_EmptyDraft()
    {
        var breakdown = QuoteScenario.Create().Calculate();

        AssertBreakdown(breakdown, subtotal: 0m, tierRate: 0m, vipRate: 0m, discount: 0m,
            taxableBase: 0m, vat: 0m, total: 0m);
    }

    [Fact(DisplayName = "C-02 · Debajo del tramo 1: 0 %; IVA ₡39.000,00; total ₡339.000,00")]
    public void C02_BelowTier1()
    {
        var breakdown = QuoteScenario.Create().WithLine(300_000m).Calculate();

        AssertBreakdown(breakdown, subtotal: 300_000m, tierRate: 0m, vipRate: 0m, discount: 0m,
            taxableBase: 300_000m, vat: 39_000m, total: 339_000m);
    }

    [Fact(DisplayName = "C-03 · Borde exacto del tramo 1: 0 %; IVA ₡65.000,00; total ₡565.000,00")]
    public void C03_ExactTier1Threshold()
    {
        var breakdown = QuoteScenario.Create().WithLine(500_000m).Calculate();

        AssertBreakdown(breakdown, subtotal: 500_000m, tierRate: 0m, vipRate: 0m, discount: 0m,
            taxableBase: 500_000m, vat: 65_000m, total: 565_000m);
    }

    [Fact(DisplayName = "C-04 · Justo sobre el tramo 1: 5 %; total ₡536.750,01")]
    public void C04_JustAboveTier1()
    {
        var breakdown = QuoteScenario.Create().WithLine(500_000.01m).Calculate();

        AssertBreakdown(breakdown, subtotal: 500_000.01m, tierRate: 5m, vipRate: 0m, discount: 25_000m,
            taxableBase: 475_000.01m, vat: 61_750m, total: 536_750.01m);
    }

    [Fact(DisplayName = "C-05 · Borde exacto del tramo 2: 5 %; total ₡1.073.500,00")]
    public void C05_ExactTier2Threshold()
    {
        var breakdown = QuoteScenario.Create().WithLine(1_000_000m).Calculate();

        AssertBreakdown(breakdown, subtotal: 1_000_000m, tierRate: 5m, vipRate: 0m, discount: 50_000m,
            taxableBase: 950_000m, vat: 123_500m, total: 1_073_500m);
    }

    [Fact(DisplayName = "C-06 · Justo sobre el tramo 2: 10 %; total ₡1.017.000,01")]
    public void C06_JustAboveTier2()
    {
        var breakdown = QuoteScenario.Create().WithLine(1_000_000.01m).Calculate();

        AssertBreakdown(breakdown, subtotal: 1_000_000.01m, tierRate: 10m, vipRate: 0m, discount: 100_000m,
            taxableBase: 900_000.01m, vat: 117_000m, total: 1_017_000.01m);
    }

    [Fact(DisplayName = "C-07 · VIP sin tramo: 3 %; total ₡219.220,00")]
    public void C07_VipWithoutTier()
    {
        var breakdown = QuoteScenario.Create().Vip().WithLine(200_000m).Calculate();

        AssertBreakdown(breakdown, subtotal: 200_000m, tierRate: 0m, vipRate: 3m, discount: 6_000m,
            taxableBase: 194_000m, vat: 25_220m, total: 219_220m);
    }

    [Fact(DisplayName = "C-08 · VIP con tramo 1: 8 %; total ₡831.680,00")]
    public void C08_VipWithTier1()
    {
        var breakdown = QuoteScenario.Create().Vip().WithLine(800_000m).Calculate();

        AssertBreakdown(breakdown, subtotal: 800_000m, tierRate: 5m, vipRate: 3m, discount: 64_000m,
            taxableBase: 736_000m, vat: 95_680m, total: 831_680m);
    }

    [Fact(DisplayName = "C-09 · VIP con tramo 2 (caso de referencia): 13 %; total ₡1.179.720,00")]
    public void C09_VipWithTier2_ReferenceCase()
    {
        var breakdown = QuoteScenario.Create().Vip().WithLine(1_200_000m).Calculate();

        AssertBreakdown(breakdown, subtotal: 1_200_000m, tierRate: 10m, vipRate: 3m, discount: 156_000m,
            taxableBase: 1_044_000m, vat: 135_720m, total: 1_179_720m);
    }

    [Fact(DisplayName = "C-10 · Redondeo de línea: 1,5 × ₡1.000,01 = ₡1.500,02")]
    public void C10_LineRounding()
    {
        var (quote, breakdown) = QuoteScenario.Create().WithLine(1_000.01m, 1.5m).Build();

        // 1.000,01 × 1,5 = 1.500,015 → 1.500,02 (el valor medio se aleja de cero).
        Assert.Equal(1_500.02m, quote.Lines.Single().CalculateSubtotal().Amount);
        Assert.Equal(1_500.02m, breakdown.Subtotal.Amount);
    }

    [Fact(DisplayName = "C-11 · Redondeo del descuento: 25.000,005 → ₡25.000,01")]
    public void C11_DiscountRounding()
    {
        var breakdown = QuoteScenario.Create().WithLine(500_000.10m).Calculate();

        Assert.Equal(5m, breakdown.TierDiscountRate.Value);
        Assert.Equal(25_000.01m, breakdown.DiscountAmount.Amount);
    }

    [Fact(DisplayName = "C-12 · Redondeo del IVA: 13.000,0065 → ₡13.000,01")]
    public void C12_VatRounding()
    {
        var breakdown = QuoteScenario.Create().WithLine(100_000.05m).Calculate();

        Assert.Equal(0m, breakdown.DiscountAmount.Amount);
        Assert.Equal(100_000.05m, breakdown.TaxableBase.Amount);
        Assert.Equal(13_000.01m, breakdown.VatAmount.Amount);
    }

    [Fact(DisplayName = "C-13 · Precio cero: total ₡0,00 y válido")]
    public void C13_ZeroPrice()
    {
        var breakdown = QuoteScenario.Create().WithLine(0m, 2m).WithLine(0m, 5m).Calculate();

        AssertBreakdown(breakdown, subtotal: 0m, tierRate: 0m, vipRate: 0m, discount: 0m,
            taxableBase: 0m, vat: 0m, total: 0m);
    }

    [Fact(DisplayName = "C-14 · Mezcla con precio cero: subtotal ₡600.000,00 con 5 %")]
    public void C14_MixedWithZeroPrice()
    {
        var breakdown = QuoteScenario.Create().WithLine(600_000m).WithLine(0m, 3m).Calculate();

        Assert.Equal(600_000m, breakdown.Subtotal.Amount);
        Assert.Equal(5m, breakdown.TierDiscountRate.Value);
    }

    [Fact(DisplayName = "C-15 · Cantidades decimales: 2,25 × ₡400.000,00 = ₡900.000,00 con 5 %")]
    public void C15_DecimalQuantity()
    {
        var (quote, breakdown) = QuoteScenario.Create().WithLine(400_000m, 2.25m).Build();

        Assert.Equal(900_000m, quote.Lines.Single().CalculateSubtotal().Amount);
        Assert.Equal(5m, breakdown.TierDiscountRate.Value);
    }

    [Fact(DisplayName = "C-16 · Varias líneas suman al tramo: subtotal ₡1.050.000,00 con 10 %")]
    public void C16_SeveralLinesReachTier2()
    {
        var breakdown = QuoteScenario.Create().WithLine(200_000m, 3m).WithLine(450_000m).Calculate();

        Assert.Equal(1_050_000m, breakdown.Subtotal.Amount);
        Assert.Equal(10m, breakdown.TierDiscountRate.Value);
    }

    [Fact(DisplayName = "C-17 · Parámetros modificados: tramo 1 en ₡400.000 → 6 % sobre ₡450.000,00")]
    public void C17_ModifiedParameters()
    {
        var settings = SystemSettings.Create(
            vatPercentage: Percentage.Create(13m),
            tier1Threshold: Money.Create(400_000m),
            tier1Discount: Percentage.Create(6m),
            tier2Threshold: Money.Create(1_000_000m),
            tier2Discount: Percentage.Create(10m),
            vipDiscount: Percentage.Create(3m),
            validityDays: 30);

        var breakdown = QuoteScenario.Create().WithSettings(settings).WithLine(450_000m).Calculate();

        // El motor usa la política recibida, no valores fijos: con la inicial serían 0 %.
        Assert.Equal(6m, breakdown.TierDiscountRate.Value);
        Assert.Equal(27_000m, breakdown.DiscountAmount.Amount);
    }

    [Fact(DisplayName = "C-18 · IVA modificado a 0 %: IVA ₡0,00 y total ₡100.000,00")]
    public void C18_ZeroVat()
    {
        var settings = SystemSettings.CreateDefault();
        settings.UpdateVatPercentage(Percentage.Create(0m));

        var breakdown = QuoteScenario.Create().WithSettings(settings).WithLine(100_000m).Calculate();

        Assert.Equal(0m, breakdown.VatRate.Value);
        Assert.Equal(0m, breakdown.VatAmount.Amount);
        Assert.Equal(100_000m, breakdown.Total.Amount);
    }

    /// <summary>Los escenarios de C-01 a C-18, para verificar la coherencia de todos (C-19).</summary>
    public static TheoryData<string> AllScenarios => new(Scenarios.Keys);

    [Theory(DisplayName = "C-19 · Coherencia del desglose")]
    [MemberData(nameof(AllScenarios))]
    public void C19_BreakdownIsConsistent(string scenario)
    {
        var breakdown = Scenarios[scenario]().Calculate();

        Assert.Equal(breakdown.Subtotal.Amount - breakdown.DiscountAmount.Amount, breakdown.TaxableBase.Amount);
        Assert.Equal(breakdown.TaxableBase.Amount + breakdown.VatAmount.Amount, breakdown.Total.Amount);
        Assert.Equal(
            breakdown.TierDiscountRate.Value + breakdown.VipDiscountRate.Value,
            breakdown.DiscountRate.Value);
    }

    private static readonly Dictionary<string, Func<QuoteScenario>> Scenarios = new()
    {
        ["C-01"] = () => QuoteScenario.Create(),
        ["C-02"] = () => QuoteScenario.Create().WithLine(300_000m),
        ["C-03"] = () => QuoteScenario.Create().WithLine(500_000m),
        ["C-04"] = () => QuoteScenario.Create().WithLine(500_000.01m),
        ["C-05"] = () => QuoteScenario.Create().WithLine(1_000_000m),
        ["C-06"] = () => QuoteScenario.Create().WithLine(1_000_000.01m),
        ["C-07"] = () => QuoteScenario.Create().Vip().WithLine(200_000m),
        ["C-08"] = () => QuoteScenario.Create().Vip().WithLine(800_000m),
        ["C-09"] = () => QuoteScenario.Create().Vip().WithLine(1_200_000m),
        ["C-10"] = () => QuoteScenario.Create().WithLine(1_000.01m, 1.5m),
        ["C-11"] = () => QuoteScenario.Create().WithLine(500_000.10m),
        ["C-12"] = () => QuoteScenario.Create().WithLine(100_000.05m),
        ["C-13"] = () => QuoteScenario.Create().WithLine(0m, 2m).WithLine(0m, 5m),
        ["C-14"] = () => QuoteScenario.Create().WithLine(600_000m).WithLine(0m, 3m),
        ["C-15"] = () => QuoteScenario.Create().WithLine(400_000m, 2.25m),
        ["C-16"] = () => QuoteScenario.Create().WithLine(200_000m, 3m).WithLine(450_000m),
        ["C-17"] = () => QuoteScenario.Create().WithLine(450_000m).WithSettings(SystemSettings.Create(
            Percentage.Create(13m), Money.Create(400_000m), Percentage.Create(6m),
            Money.Create(1_000_000m), Percentage.Create(10m), Percentage.Create(3m), 30)),
        ["C-18"] = () =>
        {
            var settings = SystemSettings.CreateDefault();
            settings.UpdateVatPercentage(Percentage.Create(0m));
            return QuoteScenario.Create().WithSettings(settings).WithLine(100_000m);
        },
    };

    private static void AssertBreakdown(
        QuoteBreakdown breakdown,
        decimal subtotal,
        decimal tierRate,
        decimal vipRate,
        decimal discount,
        decimal taxableBase,
        decimal vat,
        decimal total)
    {
        Assert.Equal(subtotal, breakdown.Subtotal.Amount);
        Assert.Equal(tierRate, breakdown.TierDiscountRate.Value);
        Assert.Equal(vipRate, breakdown.VipDiscountRate.Value);
        Assert.Equal(tierRate + vipRate, breakdown.DiscountRate.Value);
        Assert.Equal(discount, breakdown.DiscountAmount.Amount);
        Assert.Equal(taxableBase, breakdown.TaxableBase.Amount);
        Assert.Equal(13m, breakdown.VatRate.Value);
        Assert.Equal(vat, breakdown.VatAmount.Amount);
        Assert.Equal(total, breakdown.Total.Amount);
    }
}
