using System.Text;
using QuoteFlow.Application.Documents;
using static System.Net.WebUtility;

namespace QuoteFlow.Infrastructure.Documents;

internal sealed partial class QuoteDocumentRenderer
{
    /// <summary>
    /// Vista previa HTML (RF-DOC-01): página autocontenida, sin scripts ni recursos externos. Todo
    /// el texto que viene de los datos se codifica. Sigue la misma estructura que el PDF.
    /// </summary>
    public string RenderHtml(QuoteDocument document)
    {
        var html = new StringBuilder(8 * 1024);

        html.Append("<!doctype html><html lang=\"es\"><head><meta charset=\"utf-8\">")
            .Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">")
            .Append("<title>").Append(HtmlEncode(document.Title)).Append("</title>")
            .Append("<style>").Append(Styles).Append("</style></head><body>");

        if (document.Watermark is { } watermark)
        {
            html.Append("<div class=\"watermark\" aria-hidden=\"true\">")
                .Append(HtmlEncode(watermark)).Append("</div>");
        }

        html.Append("<main class=\"page\">");

        // Encabezado: empresa y título.
        html.Append("<header class=\"header\"><div>");
        AppendParty(html, document.Company, "company");
        html.Append("</div><div class=\"title\"><h1>Cotización</h1>")
            .Append("<p class=\"number\">").Append(HtmlEncode(document.NumberLabel)).Append("</p>");
        if (document.StatusStamp is { } stamp)
        {
            html.Append("<p class=\"stamp\">").Append(HtmlEncode(stamp)).Append("</p>");
        }
        if (document.Watermark is { } draftMark)
        {
            html.Append("<p class=\"draft-note\">").Append(HtmlEncode(draftMark)).Append("</p>");
        }
        html.Append("</div></header>");

        // Cliente y fechas.
        html.Append("<section class=\"info\"><div><h2>Cliente</h2>");
        AppendParty(html, document.Customer, "customer");
        html.Append("</div><dl class=\"dates\">");
        foreach (var field in document.Dates)
        {
            html.Append("<div><dt>").Append(HtmlEncode(field.Label)).Append("</dt><dd>")
                .Append(HtmlEncode(field.Value)).Append("</dd></div>");
        }
        html.Append("</dl></section>");

        AppendSections(html, document.OpeningTexts);

        // Productos.
        html.Append("<table class=\"lines\"><thead><tr>")
            .Append("<th>#</th><th>Producto</th><th class=\"num\">Cantidad</th>")
            .Append("<th class=\"num\">Precio unitario</th><th class=\"num\">Total</th>")
            .Append("</tr></thead><tbody>");
        if (document.Lines.Count == 0)
        {
            html.Append("<tr><td colspan=\"5\" class=\"empty\">Sin productos.</td></tr>");
        }
        foreach (var line in document.Lines)
        {
            html.Append("<tr><td>").Append(HtmlEncode(line.Number)).Append("</td><td>")
                .Append("<strong>").Append(HtmlEncode(line.Product)).Append("</strong>")
                .Append("<span class=\"category\">").Append(HtmlEncode(line.Category)).Append("</span>");
            if (line.Description is { } description)
            {
                html.Append("<span class=\"description\">").Append(HtmlEncode(description)).Append("</span>");
            }
            html.Append("</td><td class=\"num\">").Append(HtmlEncode(line.Quantity))
                .Append("</td><td class=\"num\">").Append(HtmlEncode(line.UnitPrice))
                .Append("</td><td class=\"num\">").Append(HtmlEncode(line.Total))
                .Append("</td></tr>");
        }
        html.Append("</tbody></table>");

        // Desglose.
        html.Append("<section class=\"totals\"><dl>");
        foreach (var amount in document.Totals)
        {
            html.Append(amount.IsGrandTotal ? "<div class=\"grand\">" : "<div>")
                .Append("<dt>").Append(HtmlEncode(amount.Label)).Append("</dt><dd>")
                .Append(HtmlEncode(amount.Value)).Append("</dd></div>");
        }
        html.Append("</dl>");
        if (document.TotalsNote is { } note)
        {
            html.Append("<p class=\"note\">").Append(HtmlEncode(note)).Append("</p>");
        }
        html.Append("</section>");

        AppendSections(html, document.ClosingTexts);

        html.Append("</main></body></html>");
        return html.ToString();
    }

    private static void AppendParty(StringBuilder html, QuoteDocumentParty party, string cssClass)
    {
        html.Append("<p class=\"").Append(cssClass).Append("\"><strong>")
            .Append(HtmlEncode(party.Name)).Append("</strong>");
        foreach (var detail in party.Details)
        {
            html.Append("<span>").Append(HtmlEncode(detail)).Append("</span>");
        }
        html.Append("</p>");
    }

    private static void AppendSections(StringBuilder html, IReadOnlyList<QuoteDocumentSection> sections)
    {
        foreach (var section in sections)
        {
            html.Append("<section class=\"text\"><h2>").Append(HtmlEncode(section.Title))
                .Append("</h2><p>").Append(HtmlEncode(section.Body)).Append("</p></section>");
        }
    }

    private const string Styles = """
        *{box-sizing:border-box}
        body{margin:0;background:#f3f4f6;color:#1a1d23;font:14px/1.5 Arial,"Helvetica Neue",sans-serif}
        .page{position:relative;max-width:816px;margin:24px auto;padding:48px 56px;background:#fff;box-shadow:0 1px 4px rgba(0,0,0,.12)}
        .watermark{position:fixed;inset:0;display:flex;align-items:center;justify-content:center;pointer-events:none;z-index:10;
          font-size:56px;font-weight:700;color:rgba(198,40,40,.16);transform:rotate(-30deg);text-align:center;white-space:nowrap}
        .header{display:flex;justify-content:space-between;gap:24px;padding-bottom:20px;border-bottom:3px solid #4f46e5}
        .company span,.customer span{display:block;color:#5f6672;font-size:13px}
        .company strong{font-size:18px}
        .title{text-align:right}
        .title h1{margin:0;font-size:26px;color:#4f46e5;letter-spacing:.02em}
        .number{margin:2px 0 0;font-size:16px;font-weight:700}
        .stamp{display:inline-block;margin:8px 0 0;padding:4px 12px;border:3px solid #c62828;color:#c62828;font-weight:800;font-size:18px;letter-spacing:.08em}
        .draft-note{margin:6px 0 0;color:#c62828;font-weight:700;font-size:12px}
        .info{display:flex;justify-content:space-between;gap:24px;margin:24px 0}
        h2{margin:0 0 6px;font-size:12px;text-transform:uppercase;letter-spacing:.06em;color:#5f6672}
        .dates{margin:0;display:grid;gap:4px;min-width:240px}
        .dates div{display:flex;justify-content:space-between;gap:16px}
        .dates dt{color:#5f6672}.dates dd{margin:0;font-weight:600}
        .text{margin:20px 0}.text p{margin:0;white-space:pre-wrap}
        .lines{width:100%;border-collapse:collapse;margin:24px 0 8px}
        .lines th{background:#f2f4f7;text-align:left;font-size:12px;text-transform:uppercase;letter-spacing:.04em;color:#5f6672;padding:8px 10px}
        .lines td{padding:9px 10px;border-bottom:1px solid #e3e6eb;vertical-align:top}
        .num{text-align:right!important;white-space:nowrap;font-variant-numeric:tabular-nums}
        .category,.description{display:block;color:#5f6672;font-size:12.5px}
        .empty{text-align:center;color:#5f6672}
        .totals{margin-left:auto;width:340px}
        .totals dl{margin:0}
        .totals div{display:flex;justify-content:space-between;gap:16px;padding:4px 0;font-variant-numeric:tabular-nums}
        .totals dt{color:#5f6672}.totals dd{margin:0}
        .totals .grand{margin-top:6px;padding-top:10px;border-top:2px solid #1a1d23;font-size:18px;font-weight:700}
        .totals .grand dt{color:#1a1d23}
        .note{margin:8px 0 0;color:#5f6672;font-size:12px}
        @media print{body{background:#fff}.page{margin:0;box-shadow:none;max-width:none}}
        """;
}
