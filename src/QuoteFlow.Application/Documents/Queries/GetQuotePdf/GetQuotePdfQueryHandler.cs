using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Application.Documents.Queries.GetQuotePdf;

internal sealed class GetQuotePdfQueryHandler(
    IQuoteRepository quotes,
    QuoteDocumentBuilder builder,
    IQuoteDocumentRenderer renderer,
    ICurrentUser currentUser)
    : IQueryHandler<GetQuotePdfQuery, QuotePdfFile>
{
    public async Task<QuotePdfFile> HandleAsync(
        GetQuotePdfQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var quote = await quotes.GetRequiredAsync(query.QuoteId, cancellationToken);

        // RN-36: solo las cotizaciones emitidas (con número) tienen PDF; un borrador, o uno
        // cancelado antes de emitirse, solo tiene vista previa.
        if (quote.Number is not { } number)
        {
            throw new DomainException(
                "El PDF solo está disponible para cotizaciones emitidas. Use la vista previa para revisar el borrador.");
        }

        var document = await builder.BuildAsync(quote, cancellationToken);

        return new QuotePdfFile($"{number}.pdf", renderer.RenderPdf(document));
    }
}
