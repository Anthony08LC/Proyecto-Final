using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Formatting;
using QuoteFlow.Application.Customers.Common;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Documents;

/// <summary>
/// Arma el modelo único del documento comercial de una cotización (RF-DOC-01). Usa los mismos
/// datos que el detalle: en los borradores, el catálogo, el cliente y la política vigentes
/// (montos estimados, RN-22); en las emitidas, los datos congelados al emitir (RN-12).
/// </summary>
internal sealed class QuoteDocumentBuilder(
    ICustomerRepository customers,
    ISystemSettingsRepository settingsRepository,
    QuoteDraftCalculator calculator)
{
    public const string DraftWatermark = "BORRADOR – Sin validez comercial";

    public async Task<QuoteDocument> BuildAsync(Quote quote, CancellationToken cancellationToken)
    {
        var customer = await customers.GetRequiredAsync(quote.CustomerId, cancellationToken);
        var settings = await settingsRepository.GetAsync(cancellationToken);
        var isDraft = quote.Status == QuoteStatus.Draft;

        if (isDraft)
        {
            // Solo para el documento: esta consulta no confirma cambios.
            await calculator.RecalculateAsync(quote, customer, cancellationToken);
        }

        return new QuoteDocument(
            Title: quote.Number is null ? "Cotización (borrador)" : $"Cotización {quote.Number}",
            NumberLabel: quote.Number ?? "Borrador",
            Watermark: isDraft ? DraftWatermark : null,
            StatusStamp: StatusStamp(quote.Status),
            Company: BuildCompany(settings),
            Customer: BuildCustomer(quote, customer),
            Dates: BuildDates(quote),
            Lines: quote.Lines
                .OrderBy(line => line.LineNumber)
                .Select(line => new QuoteDocumentLine(
                    line.LineNumber.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    line.ProductName,
                    line.ProductDescription,
                    line.CategoryName,
                    DisplayFormat.Quantity(line.Quantity),
                    DisplayFormat.Colones(line.UnitPrice.Amount),
                    DisplayFormat.Colones(line.CalculateSubtotal().Amount)))
                .ToList(),
            Totals: BuildTotals(quote),
            OpeningTexts: Sections(("Introducción", quote.Introduction), ("Resumen ejecutivo", quote.ExecutiveSummary)),
            ClosingTexts: Sections(("Observaciones", quote.Notes), ("Condiciones comerciales", quote.CommercialTerms)),
            TotalsNote: isDraft
                ? "Montos estimados con los precios y parámetros vigentes; se confirman al emitir."
                : null);
    }

    /// <summary>Estado visible cuando la cotización ya no está Emitida (RF-DOC-02).</summary>
    private static string? StatusStamp(QuoteStatus status) => status switch
    {
        QuoteStatus.Accepted => "ACEPTADA",
        QuoteStatus.Rejected => "RECHAZADA",
        QuoteStatus.Expired => "VENCIDA",
        QuoteStatus.Cancelled => "CANCELADA",
        _ => null
    };

    private static QuoteDocumentParty BuildCompany(SystemSettings settings) =>
        new(settings.CompanyName, Details(
            settings.CompanyLegalId is { } legalId ? $"Cédula jurídica: {legalId}" : null,
            settings.CompanyAddress,
            settings.CompanyEmail?.Value,
            settings.CompanyPhone is { } phone ? DisplayFormat.Phone(phone.Value) : null));

    /// <summary>En borradores, los datos vigentes del cliente; una vez emitida, los congelados (RN-12).</summary>
    private static QuoteDocumentParty BuildCustomer(Quote quote, Customer customer)
    {
        if (quote.CustomerName is { } frozenName)
        {
            return new QuoteDocumentParty(frozenName, Details(
                quote.CustomerCompany,
                quote.CustomerEmail?.Value,
                quote.CustomerPhone is { } frozenPhone ? DisplayFormat.Phone(frozenPhone.Value) : null));
        }

        return new QuoteDocumentParty(customer.Name, Details(
            customer.Company,
            customer.Email?.Value,
            customer.Phone is { } phone ? DisplayFormat.Phone(phone.Value) : null));
    }

    private static List<QuoteDocumentField> BuildDates(Quote quote) =>
    [
        new("Número", quote.Number ?? "Borrador (sin número)"),
        new("Fecha de emisión", quote.IssuedAt is { } issuedAt
            ? DisplayFormat.Date(issuedAt)
            : "Pendiente de emisión"),
        new("Vigente hasta", quote.ValidUntil is { } validUntil
            ? DisplayFormat.Date(validUntil)
            : "Se define al emitir")
    ];

    private static List<QuoteDocumentAmount> BuildTotals(Quote quote)
    {
        var breakdown = quote.Breakdown;
        var totals = new List<QuoteDocumentAmount>
        {
            new("Subtotal", DisplayFormat.Colones(breakdown.Subtotal.Amount))
        };

        if (breakdown.DiscountAmount.Amount > 0)
        {
            totals.Add(new($"Descuento ({DiscountDescription(quote)})",
                "−" + DisplayFormat.Colones(breakdown.DiscountAmount.Amount)));
        }

        totals.Add(new("Base imponible", DisplayFormat.Colones(breakdown.TaxableBase.Amount)));
        totals.Add(new($"IVA ({DisplayFormat.Percent(breakdown.VatRate.Value)})",
            DisplayFormat.Colones(breakdown.VatAmount.Amount)));
        totals.Add(new("Total", DisplayFormat.Colones(breakdown.Total.Amount), IsGrandTotal: true));

        return totals;
    }

    /// <summary>Por ejemplo: «10 % por volumen + 3 % VIP».</summary>
    private static string DiscountDescription(Quote quote)
    {
        var parts = new List<string>();
        if (quote.Breakdown.TierDiscountRate.Value > 0)
        {
            parts.Add($"{DisplayFormat.Percent(quote.Breakdown.TierDiscountRate.Value)} por volumen");
        }
        if (quote.Breakdown.VipDiscountRate.Value > 0)
        {
            parts.Add($"{DisplayFormat.Percent(quote.Breakdown.VipDiscountRate.Value)} VIP");
        }
        return string.Join(" + ", parts);
    }

    /// <summary>Solo los textos comerciales que tienen contenido (RF-DOC-01).</summary>
    private static List<QuoteDocumentSection> Sections(params (string Title, string? Body)[] texts) =>
        texts
            .Where(text => !string.IsNullOrWhiteSpace(text.Body))
            .Select(text => new QuoteDocumentSection(text.Title, text.Body!.Trim()))
            .ToList();

    private static List<string> Details(params string?[] values) =>
        values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!).ToList();
}
