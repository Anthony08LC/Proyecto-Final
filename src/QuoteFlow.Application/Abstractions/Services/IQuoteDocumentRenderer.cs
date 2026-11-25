using QuoteFlow.Application.Documents;

namespace QuoteFlow.Application.Abstractions.Services;

/// <summary>
/// Genera el documento comercial a partir del modelo único (RF-DOC-01, RF-DOC-02): la vista previa
/// HTML y el PDF comparten datos y estructura.
/// </summary>
public interface IQuoteDocumentRenderer
{
    /// <summary>Página HTML completa y autocontenida (sin scripts ni recursos externos).</summary>
    string RenderHtml(QuoteDocument document);

    byte[] RenderPdf(QuoteDocument document);
}
