using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Formatting;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Customers.Common;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Common;

namespace QuoteFlow.Application.Quotes.Commands.IssueQuote;

internal sealed class IssueQuoteCommandHandler(
    IQuoteRepository quotes,
    ICustomerRepository customers,
    ISystemSettingsRepository settingsRepository,
    IQuoteNumberGenerator numberGenerator,
    QuoteDraftCalculator calculator,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog,
    TimeProvider timeProvider)
    : ICommandHandler<IssueQuoteCommand, IssueQuoteResult>
{
    public Task<IssueQuoteResult> HandleAsync(
        IssueQuoteCommand command,
        CancellationToken cancellationToken = default)
    {
        return unitOfWork.ExecuteInTransactionAsync(
            ct => IssueAsync(command.QuoteId, ct),
            cancellationToken);
    }

    private async Task<IssueQuoteResult> IssueAsync(Guid quoteId, CancellationToken cancellationToken)
    {
        var quote = await quotes.GetRequiredAsync(quoteId, cancellationToken);
        currentUser.EnsureCanManage(quote);

        var customer = await customers.GetRequiredAsync(quote.CustomerId, cancellationToken);
        var catalog = await calculator.LoadCatalogAsync(quote, cancellationToken);

        // Se valida antes de reservar el número, para no bloquear la secuencia sin necesidad.
        quote.EnsureCanEmit(customer, catalog.Products);

        var settings = await settingsRepository.GetAsync(cancellationToken);
        var now = timeProvider.GetUtcNow();
        var number = await numberGenerator.NextAsync(CostaRicaTime.ToLocalDate(now).Year, cancellationToken);

        quote.Emit(number, customer, catalog.Products, catalog.Categories, settings, now);

        auditLog.Record(new AuditEntry(
            QuoteAuditEvents.Issued,
            AuditEntityType.Quote,
            quote.Id,
            $"Cotización {number} emitida para «{customer.Name}» por un total de "
            + $"{DisplayFormat.Colones(quote.Breakdown.Total.Amount)}; vigente hasta {DisplayFormat.Date(quote.ValidUntil!.Value)}."));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new IssueQuoteResult(number, quote.IssuedAt!.Value, quote.ValidUntil!.Value);
    }
}
