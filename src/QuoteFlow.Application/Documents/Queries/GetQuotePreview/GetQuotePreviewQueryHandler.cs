using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application.Documents.Queries.GetQuotePreview;

internal sealed class GetQuotePreviewQueryHandler(
    IQuoteRepository quotes,
    QuoteDocumentBuilder builder,
    IQuoteDocumentRenderer renderer,
    ICurrentUser currentUser)
    : IQueryHandler<GetQuotePreviewQuery, string>
{
    public async Task<string> HandleAsync(
        GetQuotePreviewQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var quote = await quotes.GetRequiredAsync(query.QuoteId, cancellationToken);
        var document = await builder.BuildAsync(quote, cancellationToken);

        return renderer.RenderHtml(document);
    }
}
