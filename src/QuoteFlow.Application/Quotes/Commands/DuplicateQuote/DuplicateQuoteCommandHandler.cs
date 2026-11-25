using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Formatting;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Customers.Common;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application.Quotes.Commands.DuplicateQuote;

internal sealed class DuplicateQuoteCommandHandler(
    IQuoteRepository quotes,
    ICustomerRepository customers,
    ISystemSettingsRepository settingsRepository,
    QuoteDraftCalculator calculator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<DuplicateQuoteCommand, DuplicateQuoteResult>
{
    public async Task<DuplicateQuoteResult> HandleAsync(
        DuplicateQuoteCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var source = await quotes.GetRequiredAsync(command.QuoteId, cancellationToken);
        var customer = await customers.GetRequiredAsync(source.CustomerId, cancellationToken);

        if (!customer.IsActive)
        {
            throw new ConflictException(
                $"No se puede duplicar: el cliente «{customer.Name}» está inactivo.");
        }

        var catalog = await calculator.LoadCatalogAsync(source, cancellationToken);
        var settings = await settingsRepository.GetAsync(cancellationToken);

        var duplication = source.Duplicate(
            customer,
            currentUser.UserId,
            catalog.Products,
            catalog.Categories,
            settings);

        var copy = duplication.Copy;
        quotes.Add(copy);

        var detail = $"Borrador creado como copia de la cotización {source.DisplayNumber()} para «{customer.Name}».";

        if (duplication.OmittedProducts.Count > 0)
        {
            detail += $" Productos inactivos omitidos: {string.Join(", ", duplication.OmittedProducts)}.";
        }

        auditLog.Record(new AuditEntry(
            QuoteAuditEvents.Duplicated,
            AuditEntityType.Quote,
            copy.Id,
            DisplayFormat.Truncate(detail, AuditEntry.DetailMaxLength)));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new DuplicateQuoteResult(copy.Id, duplication.OmittedProducts);
    }
}
