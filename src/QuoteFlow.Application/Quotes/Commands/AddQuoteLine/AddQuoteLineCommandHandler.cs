using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Customers.Common;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application.Quotes.Commands.AddQuoteLine;

internal sealed class AddQuoteLineCommandHandler(
    IQuoteRepository quotes,
    IProductRepository products,
    ICategoryRepository categories,
    ICustomerRepository customers,
    QuoteDraftCalculator calculator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<AddQuoteLineCommand, QuoteBreakdownDto>
{
    public async Task<QuoteBreakdownDto> HandleAsync(
        AddQuoteLineCommand command,
        CancellationToken cancellationToken = default)
    {
        var quote = await quotes.GetRequiredAsync(command.QuoteId, cancellationToken);
        currentUser.EnsureCanManage(quote);

        QuoteInput.EnsureValidQuantity(command.Quantity);

        var product = await products.GetByIdAsync(command.ProductId, cancellationToken)
            ?? throw new ValidationException("productId", "El producto seleccionado no existe.");

        if (!product.IsActive)
        {
            throw new ValidationException(
                "productId",
                $"El producto «{product.Name}» está inactivo y no se puede agregar.");
        }

        var category = await categories.GetByIdAsync(product.CategoryId, cancellationToken)
            ?? throw new NotFoundException("la categoría", product.CategoryId);

        quote.AddLine(product, category, command.Quantity);

        var customer = await customers.GetRequiredAsync(quote.CustomerId, cancellationToken);
        await calculator.RecalculateAsync(quote, customer, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return quote.BreakdownDto();
    }
}
