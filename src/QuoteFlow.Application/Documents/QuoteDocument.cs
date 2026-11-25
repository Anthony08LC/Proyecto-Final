namespace QuoteFlow.Application.Documents;

/// <summary>
/// Modelo único del documento comercial (RF-DOC-01, RF-DOC-02). La vista previa HTML y el PDF se
/// generan a partir de este mismo modelo, con los textos ya formateados (colones, fechas
/// dd/mm/aaaa), para que su contenido sea idéntico.
/// </summary>
/// <param name="Watermark">Marca de agua de los borradores (RN-36); nula en las emitidas.</param>
/// <param name="StatusStamp">Sello visible si la cotización ya no está Emitida (RF-DOC-02), por ejemplo «VENCIDA».</param>
/// <param name="OpeningTexts">Introducción y resumen ejecutivo, antes de los productos.</param>
/// <param name="ClosingTexts">Observaciones y condiciones comerciales, después del desglose.</param>
/// <param name="TotalsNote">Nota al pie del desglose (por ejemplo, que los montos son estimados).</param>
public sealed record QuoteDocument(
    string Title,
    string NumberLabel,
    string? Watermark,
    string? StatusStamp,
    QuoteDocumentParty Company,
    QuoteDocumentParty Customer,
    IReadOnlyList<QuoteDocumentField> Dates,
    IReadOnlyList<QuoteDocumentLine> Lines,
    IReadOnlyList<QuoteDocumentAmount> Totals,
    IReadOnlyList<QuoteDocumentSection> OpeningTexts,
    IReadOnlyList<QuoteDocumentSection> ClosingTexts,
    string? TotalsNote);

/// <summary>Empresa o cliente: nombre y líneas de datos de contacto que existan.</summary>
public sealed record QuoteDocumentParty(string Name, IReadOnlyList<string> Details);

public sealed record QuoteDocumentField(string Label, string Value);

public sealed record QuoteDocumentLine(
    string Number,
    string Product,
    string? Description,
    string Category,
    string Quantity,
    string UnitPrice,
    string Total);

public sealed record QuoteDocumentAmount(string Label, string Value, bool IsGrandTotal = false);

/// <summary>Texto comercial con contenido (los vacíos no se incluyen).</summary>
public sealed record QuoteDocumentSection(string Title, string Body);

/// <summary>PDF generado, con su nombre de archivo <c>COT-AAAA-000001.pdf</c>.</summary>
public sealed record QuotePdfFile(string FileName, byte[] Content);
