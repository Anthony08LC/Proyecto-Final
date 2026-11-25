using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Documents;

namespace QuoteFlow.Infrastructure.Documents;

/// <summary>
/// Documento comercial de una cotización (RF-DOC-01, RF-DOC-02). La vista previa HTML y el PDF se
/// generan del mismo <see cref="QuoteDocument"/>, con los mismos textos ya formateados, en el mismo
/// orden: encabezado, cliente y fechas, textos iniciales, productos, desglose y textos finales.
/// </summary>
internal sealed partial class QuoteDocumentRenderer : IQuoteDocumentRenderer
{
    private const string Primary = "#4F46E5";
    private const string Muted = "#5F6672";
    private const string Danger = "#C62828";
    private const string Border = "#E3E6EB";

    /// <summary>Rojo con transparencia para la marca de agua; QuestPDF usa el formato #AARRGGBB.</summary>
    private const string WatermarkColor = "#33C62828";

    /// <summary>
    /// Lato viene incluida en QuestPDF y tiene el símbolo del colón (₡). Desde la versión 2026.9,
    /// QuestPDF no usa las fuentes del sistema: el PDF es igual en Windows y en Ubuntu Server.
    /// </summary>
    private const string FontFamily = "Lato";

    static QuoteDocumentRenderer()
    {
        // Licencia Community de QuestPDF: gratuita para organizaciones con ingresos anuales
        // menores al umbral de la licencia.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] RenderPdf(QuoteDocument document) =>
        Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.Letter);
            page.Margin(40);
            page.DefaultTextStyle(style => style.FontFamily(FontFamily).FontSize(10).FontColor("#1A1D23"));

            if (document.Watermark is { } watermark)
            {
                page.Foreground()
                    .AlignCenter()
                    .AlignMiddle()
                    .Rotate(-30)
                    .Text(watermark)
                    .FontSize(44)
                    .Bold()
                    .FontColor(WatermarkColor);
            }

            page.Header().Element(header => ComposeHeader(header, document));
            page.Content().PaddingTop(16).Element(content => ComposeContent(content, document));
            page.Footer().AlignRight().Text(text =>
            {
                text.DefaultTextStyle(style => style.FontSize(8).FontColor(Muted));
                text.Span($"{document.NumberLabel} · Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        }))
        .WithMetadata(new DocumentMetadata { Title = document.Title, Author = document.Company.Name })
        .GeneratePdf();

    private static void ComposeHeader(IContainer container, QuoteDocument document)
    {
        container.BorderBottom(3).BorderColor(Primary).PaddingBottom(12).Row(row =>
        {
            row.RelativeItem().Column(column => ComposeParty(column, document.Company, nameSize: 14));

            row.ConstantItem(220).AlignRight().Column(column =>
            {
                column.Item().AlignRight().Text("Cotización").FontSize(20).Bold().FontColor(Primary);
                column.Item().AlignRight().Text(document.NumberLabel).FontSize(12).Bold();

                if (document.StatusStamp is { } stamp)
                {
                    column.Item().PaddingTop(6).AlignRight()
                        .Border(2).BorderColor(Danger).PaddingVertical(3).PaddingHorizontal(10)
                        .Text(stamp).FontSize(14).Bold().FontColor(Danger);
                }

                if (document.Watermark is { } draftMark)
                {
                    column.Item().PaddingTop(4).AlignRight().Text(draftMark).FontSize(8).Bold().FontColor(Danger);
                }
            });
        });
    }

    private static void ComposeContent(IContainer container, QuoteDocument document)
    {
        container.Column(column =>
        {
            column.Spacing(14);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(customer =>
                {
                    customer.Item().Text("CLIENTE").FontSize(8).Bold().FontColor(Muted);
                    ComposeParty(customer, document.Customer, nameSize: 11);
                });

                row.ConstantItem(210).Column(dates =>
                {
                    foreach (var field in document.Dates)
                    {
                        dates.Item().Row(line =>
                        {
                            line.RelativeItem().Text(field.Label).FontColor(Muted);
                            line.AutoItem().Text(field.Value).SemiBold();
                        });
                    }
                });
            });

            ComposeSections(column, document.OpeningTexts);
            column.Item().Element(lines => ComposeLines(lines, document.Lines));
            column.Item().AlignRight().Width(300).Element(totals => ComposeTotals(totals, document));
            ComposeSections(column, document.ClosingTexts);
        });
    }

    private static void ComposeParty(ColumnDescriptor column, QuoteDocumentParty party, float nameSize)
    {
        column.Item().Text(party.Name).FontSize(nameSize).Bold();
        foreach (var detail in party.Details)
        {
            column.Item().Text(detail).FontSize(9).FontColor(Muted);
        }
    }

    private static void ComposeSections(ColumnDescriptor column, IReadOnlyList<QuoteDocumentSection> sections)
    {
        foreach (var section in sections)
        {
            column.Item().Column(text =>
            {
                text.Item().Text(section.Title.ToUpperInvariant()).FontSize(8).Bold().FontColor(Muted);
                text.Item().PaddingTop(2).Text(section.Body);
            });
        }
    }

    private static void ComposeLines(IContainer container, IReadOnlyList<QuoteDocumentLine> lines)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(24);
                columns.RelativeColumn(5);
                columns.RelativeColumn(1.4f);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "#", alignRight: false);
                HeaderCell(header.Cell(), "PRODUCTO", alignRight: false);
                HeaderCell(header.Cell(), "CANTIDAD", alignRight: true);
                HeaderCell(header.Cell(), "PRECIO UNITARIO", alignRight: true);
                HeaderCell(header.Cell(), "TOTAL", alignRight: true);
            });

            if (lines.Count == 0)
            {
                table.Cell().ColumnSpan(5).Element(BodyCell).AlignCenter().Text("Sin productos.").FontColor(Muted);
            }

            foreach (var line in lines)
            {
                table.Cell().Element(BodyCell).Text(line.Number);
                table.Cell().Element(BodyCell).Column(product =>
                {
                    product.Item().Text(line.Product).SemiBold();
                    product.Item().Text(line.Category).FontSize(8.5f).FontColor(Muted);
                    if (line.Description is { } description)
                    {
                        product.Item().Text(description).FontSize(8.5f).FontColor(Muted);
                    }
                });
                table.Cell().Element(BodyCell).AlignRight().Text(line.Quantity);
                table.Cell().Element(BodyCell).AlignRight().Text(line.UnitPrice);
                table.Cell().Element(BodyCell).AlignRight().Text(line.Total);
            }
        });

        static void HeaderCell(IContainer cell, string text, bool alignRight)
        {
            var container = cell.Background("#F2F4F7").PaddingVertical(5).PaddingHorizontal(6);
            (alignRight ? container.AlignRight() : container).Text(text).FontSize(8).Bold().FontColor(Muted);
        }

        static IContainer BodyCell(IContainer cell) =>
            cell.BorderBottom(1).BorderColor(Border).PaddingVertical(6).PaddingHorizontal(6);
    }

    private static void ComposeTotals(IContainer container, QuoteDocument document)
    {
        container.Column(column =>
        {
            foreach (var amount in document.Totals)
            {
                var item = amount.IsGrandTotal
                    ? column.Item().PaddingTop(6).BorderTop(2).PaddingTop(6)
                    : column.Item().PaddingVertical(2);

                item.Row(row =>
                {
                    var label = row.RelativeItem().Text(amount.Label);
                    var value = row.AutoItem().Text(amount.Value);

                    if (amount.IsGrandTotal)
                    {
                        label.FontSize(13).Bold();
                        value.FontSize(13).Bold();
                    }
                    else
                    {
                        label.FontColor(Muted);
                    }
                });
            }

            if (document.TotalsNote is { } note)
            {
                column.Item().PaddingTop(6).Text(note).FontSize(8).FontColor(Muted);
            }
        });
    }
}
