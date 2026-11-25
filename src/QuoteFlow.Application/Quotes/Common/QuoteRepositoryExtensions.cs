using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Quotes.Common;

internal static class QuoteRepositoryExtensions
{
    public static async Task<Quote> GetRequiredAsync(
        this IQuoteRepository repository,
        Guid quoteId,
        CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(quoteId, cancellationToken)
            ?? throw new NotFoundException("la cotización", quoteId);
    }
}
