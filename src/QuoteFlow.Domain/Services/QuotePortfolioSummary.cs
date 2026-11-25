using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Domain.Services;

/// <summary>
/// Totales de las cotizaciones en un estado: cuántas hay, cuántas tienen número y la suma de sus
/// totales. Es la materia prima de los indicadores del dashboard (RF-DSH-01).
/// </summary>
public sealed record QuoteStatusTotals(QuoteStatus Status, int Count, int NumberedCount, Money Total);

/// <summary>
/// Indicadores de cotizaciones del dashboard (RF-DSH-01), según las definiciones de la especificación:
/// <list type="bullet">
///   <item>Cotizaciones con número: todas menos los borradores (un borrador cancelado nunca tuvo número).</item>
///   <item>Monto total cotizado: suma de los totales de las Emitidas, Aceptadas, Rechazadas y Vencidas;
///   excluye borradores y canceladas.</item>
/// </list>
/// </summary>
public sealed record QuotePortfolioSummary(int NumberedQuotes, Money QuotedAmount)
{
    /// <summary>Estados cuyo total cuenta para el monto cotizado.</summary>
    public static IReadOnlySet<QuoteStatus> QuotedAmountStatuses { get; } = new HashSet<QuoteStatus>
    {
        QuoteStatus.Issued,
        QuoteStatus.Accepted,
        QuoteStatus.Rejected,
        QuoteStatus.Expired,
    };

    public static QuotePortfolioSummary From(IEnumerable<QuoteStatusTotals> totals)
    {
        ArgumentNullException.ThrowIfNull(totals);

        var numbered = 0;
        var amount = Money.Zero;

        foreach (var row in totals)
        {
            if (row.Status != QuoteStatus.Draft)
            {
                numbered += row.NumberedCount;
            }

            if (QuotedAmountStatuses.Contains(row.Status))
            {
                amount = amount.Add(row.Total);
            }
        }

        return new QuotePortfolioSummary(numbered, amount);
    }
}
