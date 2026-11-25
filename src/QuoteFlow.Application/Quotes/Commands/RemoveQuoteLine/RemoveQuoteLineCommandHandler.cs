using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Customers.Common;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application.Quotes.Commands.RemoveQuoteLine;

internal sealed class RemoveQuoteLineCommandHandler(
    IQuoteRepository quotes,
    ICustomerRepository customers,
    QuoteDraftCalculator calculator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<RemoveQuoteLineCommand, QuoteBreakdownDto>
{
    public async Task<QuoteBreakdownDto> HandleAsync(
        RemoveQuoteLineCommand command,
        CancellationToken cancellationToken = default)
    {
        var quote = await quotes.GetRequiredAsync(command.QuoteId, cancellationToken);
        currentUser.EnsureCanManage(quote);

        quote.RemoveLine(command.ProductId);

        var customer = await customers.GetRequiredAsync(quote.CustomerId, cancellationToken);
        await calculator.RecalculateAsync(quote, customer, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return quote.BreakdownDto();
    }
}
