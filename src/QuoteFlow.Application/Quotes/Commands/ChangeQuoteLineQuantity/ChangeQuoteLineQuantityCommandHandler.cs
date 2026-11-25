using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Customers.Common;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application.Quotes.Commands.ChangeQuoteLineQuantity;

internal sealed class ChangeQuoteLineQuantityCommandHandler(
    IQuoteRepository quotes,
    ICustomerRepository customers,
    QuoteDraftCalculator calculator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<ChangeQuoteLineQuantityCommand, QuoteBreakdownDto>
{
    public async Task<QuoteBreakdownDto> HandleAsync(
        ChangeQuoteLineQuantityCommand command,
        CancellationToken cancellationToken = default)
    {
        var quote = await quotes.GetRequiredAsync(command.QuoteId, cancellationToken);
        currentUser.EnsureCanManage(quote);

        QuoteInput.EnsureValidQuantity(command.Quantity);

        quote.ChangeLineQuantity(command.ProductId, command.Quantity);

        var customer = await customers.GetRequiredAsync(quote.CustomerId, cancellationToken);
        await calculator.RecalculateAsync(quote, customer, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return quote.BreakdownDto();
    }
}
