using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Customers.Common;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Quotes.Queries.GetQuoteById;

internal sealed class GetQuoteByIdQueryHandler(
    IQuoteRepository quotes,
    ICustomerRepository customers,
    IUserRepository users,
    IAuditEventReader auditEvents,
    QuoteDraftCalculator calculator,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IQueryHandler<GetQuoteByIdQuery, QuoteDetailDto>
{
    public async Task<QuoteDetailDto> HandleAsync(
        GetQuoteByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var quote = await quotes.GetRequiredAsync(query.QuoteId, cancellationToken);
        var customer = await customers.GetRequiredAsync(quote.CustomerId, cancellationToken);
        var isDraft = quote.Status == QuoteStatus.Draft;

        // Los borradores siempre reflejan el catálogo y la política vigentes (RN-22).
        // El recálculo es solo para la respuesta: esta consulta no confirma cambios.
        var catalog = isDraft
            ? await calculator.RecalculateAsync(quote, customer, cancellationToken)
            : QuoteCatalog.Empty;

        var owner = await users.GetByIdAsync(quote.OwnerId, cancellationToken);

        string? sourceNumber = null;
        if (quote.SourceQuoteId is { } sourceId)
        {
            var source = await quotes.GetByIdAsync(sourceId, cancellationToken);
            sourceNumber = source?.DisplayNumber();
        }

        var history = await auditEvents.GetByEntityAsync(AuditEntityType.Quote, quote.Id, cancellationToken);

        var lines = quote.Lines
            .OrderBy(l => l.LineNumber)
            .Select(l => l.ToDto(isDraft ? catalog.IsProductActive(l.ProductId) : null))
            .ToList();

        return new QuoteDetailDto(
            quote.Id,
            quote.Number,
            quote.Status,
            BuildCustomer(quote, customer),
            quote.OwnerId,
            owner?.FullName ?? string.Empty,
            quote.SourceQuoteId,
            sourceNumber,
            quote.Introduction,
            quote.ExecutiveSummary,
            quote.Notes,
            quote.CommercialTerms,
            lines,
            quote.BreakdownDto(),
            quote.IssuedAt,
            quote.ValidUntil,
            quote.ClosedAt,
            quote.CloseReason,
            quote.CreatedAt,
            quote.UpdatedAt,
            BuildActions(quote, customer, catalog),
            history.Select(e => new QuoteHistoryEntryDto(e.OccurredAt, e.UserName, e.EventType, e.Detail)).ToList());
    }

    /// <summary>
    /// En borradores se muestran los datos vigentes del cliente; una vez emitida, los congelados (RN-12).
    /// </summary>
    private static QuoteCustomerDto BuildCustomer(Quote quote, Customer customer) =>
        quote.CustomerName is null
            ? new QuoteCustomerDto(
                customer.Id,
                customer.Name,
                customer.Company,
                customer.Email?.Value,
                customer.Phone?.Value,
                customer.IsVip,
                customer.IsActive)
            : new QuoteCustomerDto(
                customer.Id,
                quote.CustomerName,
                quote.CustomerCompany,
                quote.CustomerEmail?.Value,
                quote.CustomerPhone?.Value,
                quote.CustomerIsVip ?? false,
                customer.IsActive);

    private QuoteActionsDto BuildActions(Quote quote, Customer customer, QuoteCatalog catalog)
    {
        var canManage = currentUser.CanManage(quote);
        var isDraft = quote.Status == QuoteStatus.Draft;
        var isIssued = quote.Status == QuoteStatus.Issued;

        var canIssue = isDraft
            && canManage
            && quote.Lines.Count > 0
            && customer.IsActive
            && quote.Lines.All(l => catalog.IsProductActive(l.ProductId));

        return new QuoteActionsDto(
            CanEdit: isDraft && canManage,
            CanIssue: canIssue,
            CanAccept: isIssued && canManage && quote.IsWithinValidity(timeProvider.GetUtcNow()),
            CanReject: isIssued && canManage,
            CanCancel: (isDraft || isIssued) && canManage,
            CanDuplicate: customer.IsActive,
            CanDownloadPdf: quote.Number is not null,
            CanUseAssistant: isDraft && canManage);
    }
}
