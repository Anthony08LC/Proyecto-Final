using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Common;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Quotes.Queries.GetQuotes;

internal sealed class GetQuotesQueryHandler(
    IQuoteRepository quotes,
    ICustomerRepository customers,
    IUserRepository users,
    ICurrentUser currentUser)
    : IQueryHandler<GetQuotesQuery, PagedResult<QuoteSummaryDto>>
{
    public async Task<PagedResult<QuoteSummaryDto>> HandleAsync(
        GetQuotesQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        if (query.From is { } from && query.To is { } to && from > to)
        {
            throw new ValidationException("from", "La fecha inicial no puede ser posterior a la fecha final.");
        }

        var criteria = new QuoteSearchCriteria(
            string.IsNullOrWhiteSpace(query.Number) ? null : query.Number.Trim(),
            query.CustomerId,
            query.Status,
            query.OwnerId,
            query.From is { } start ? StartOfDay(start) : null,
            query.To is { } end ? StartOfDay(end.AddDays(1)) : null);

        var page = await quotes.SearchAsync(
            criteria,
            new PageRequest(query.Page, query.PageSize),
            cancellationToken);

        var customerIds = page.Items.Select(q => q.CustomerId).Distinct().ToList();
        var ownerIds = page.Items.Select(q => q.OwnerId).Distinct().ToList();

        var customersById = customerIds.Count == 0
            ? []
            : (await customers.GetByIdsAsync(customerIds, cancellationToken)).ToDictionary(c => c.Id);

        var ownersById = ownerIds.Count == 0
            ? []
            : (await users.GetByIdsAsync(ownerIds, cancellationToken)).ToDictionary(u => u.Id);

        var items = page.Items
            .Select(q => new QuoteSummaryDto(
                q.Id,
                q.Number,
                q.Status,
                q.CustomerId,
                q.CustomerName ?? customersById.GetValueOrDefault(q.CustomerId)?.Name ?? string.Empty,
                q.OwnerId,
                ownersById.GetValueOrDefault(q.OwnerId)?.FullName ?? string.Empty,
                q.Breakdown.Total.Amount,
                q.Status == QuoteStatus.Draft,
                q.CreatedAt,
                q.IssuedAt,
                q.ValidUntil))
            .ToList();

        return new PagedResult<QuoteSummaryDto>(items, page.Page, page.PageSize, page.TotalCount);
    }

    /// <summary>Inicio del día indicado en hora de Costa Rica (RNF-DAT-03).</summary>
    private static DateTimeOffset StartOfDay(DateOnly date) =>
        new(date.ToDateTime(TimeOnly.MinValue), CostaRicaTime.Offset);
}
