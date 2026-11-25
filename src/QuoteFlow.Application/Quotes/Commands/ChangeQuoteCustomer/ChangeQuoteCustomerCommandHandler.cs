using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application.Quotes.Commands.ChangeQuoteCustomer;

internal sealed class ChangeQuoteCustomerCommandHandler(
    IQuoteRepository quotes,
    ICustomerRepository customers,
    QuoteDraftCalculator calculator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<ChangeQuoteCustomerCommand, QuoteBreakdownDto>
{
    public async Task<QuoteBreakdownDto> HandleAsync(
        ChangeQuoteCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        var quote = await quotes.GetRequiredAsync(command.QuoteId, cancellationToken);
        currentUser.EnsureCanManage(quote);

        var customer = await customers.GetByIdAsync(command.CustomerId, cancellationToken)
            ?? throw new ValidationException("customerId", "El cliente seleccionado no existe.");

        if (!customer.IsActive)
        {
            throw new ValidationException(
                "customerId",
                $"El cliente «{customer.Name}» está inactivo y no se puede asignar a la cotización.");
        }

        quote.ChangeCustomer(customer);
        await calculator.RecalculateAsync(quote, customer, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return quote.BreakdownDto();
    }
}
