using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.Services;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Domain.Tests.Services;

/// <summary>Indicadores de cotizaciones del dashboard con datos conocidos (RF-DSH-01, T-084).</summary>
public sealed class QuotePortfolioSummaryTests
{
    private static QuoteStatusTotals Row(QuoteStatus status, int count, int numbered, decimal total) =>
        new(status, count, numbered, Money.Create(total));

    // Un estado de cada tipo, con totales distintos para detectar cuál se suma por error.
    private static readonly QuoteStatusTotals[] KnownData =
    [
        Row(QuoteStatus.Draft, count: 4, numbered: 0, total: 1_000_000m),
        Row(QuoteStatus.Issued, count: 3, numbered: 3, total: 300_000m),
        Row(QuoteStatus.Accepted, count: 2, numbered: 2, total: 200_000.50m),
        Row(QuoteStatus.Rejected, count: 1, numbered: 1, total: 10_000m),
        Row(QuoteStatus.Expired, count: 1, numbered: 1, total: 5_000m),
        // 1 borrador cancelado (sin número) y 1 emitida cancelada (con número).
        Row(QuoteStatus.Cancelled, count: 2, numbered: 1, total: 700_000m),
    ];

    [Fact(DisplayName = "DSH · Sin cotizaciones: 0 con número y ₡0,00 cotizados")]
    public void Empty()
    {
        var summary = QuotePortfolioSummary.From([]);

        Assert.Equal(0, summary.NumberedQuotes);
        Assert.Equal(0m, summary.QuotedAmount.Amount);
    }

    [Fact(DisplayName = "DSH · Cotizaciones con número: todas menos los borradores")]
    public void NumberedQuotes_ExcludeDrafts()
    {
        var summary = QuotePortfolioSummary.From(KnownData);

        // 3 emitidas + 2 aceptadas + 1 rechazada + 1 vencida + 1 cancelada que se había emitido.
        Assert.Equal(8, summary.NumberedQuotes);
    }

    [Fact(DisplayName = "DSH · Monto cotizado: Emitidas, Aceptadas, Rechazadas y Vencidas; sin borradores ni canceladas")]
    public void QuotedAmount_ExcludesDraftsAndCancelled()
    {
        var summary = QuotePortfolioSummary.From(KnownData);

        Assert.Equal(515_000.50m, summary.QuotedAmount.Amount);
    }

    [Theory(DisplayName = "DSH · Estados que cuentan para el monto cotizado")]
    [InlineData(QuoteStatus.Draft, false)]
    [InlineData(QuoteStatus.Issued, true)]
    [InlineData(QuoteStatus.Accepted, true)]
    [InlineData(QuoteStatus.Rejected, true)]
    [InlineData(QuoteStatus.Expired, true)]
    [InlineData(QuoteStatus.Cancelled, false)]
    public void QuotedAmountStatuses(QuoteStatus status, bool counts)
    {
        Assert.Equal(counts, QuotePortfolioSummary.QuotedAmountStatuses.Contains(status));
    }
}
