using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Dashboard.Common;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.Services;

namespace QuoteFlow.Application.Dashboard.Queries.GetDashboard;

internal sealed class GetDashboardQueryHandler(
    IQuoteRepository quotes,
    ICustomerRepository customers,
    IProductRepository products,
    IAuditEventReader auditEvents,
    ICurrentUser currentUser)
    : IQueryHandler<GetDashboardQuery, DashboardDto>
{
    /// <summary>Cotizaciones recientes que se muestran (RF-DSH-02).</summary>
    public const int RecentQuotesCount = 5;

    /// <summary>Eventos recientes que se muestran (RF-DSH-03).</summary>
    public const int RecentActivityCount = 10;

    public async Task<DashboardDto> HandleAsync(
        GetDashboardQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        // Consultas secuenciales: comparten el mismo DbContext.
        var activeCustomers = await customers.CountActiveAsync(cancellationToken);
        var activeProducts = await products.CountActiveAsync(cancellationToken);
        var portfolio = QuotePortfolioSummary.From(await quotes.GetTotalsByStatusAsync(cancellationToken));

        var recent = await quotes.SearchAsync(
            new QuoteSearchCriteria(null, null, null, null, null, null),
            new PageRequest(1, RecentQuotesCount),
            cancellationToken);

        var customerIds = recent.Items
            .Where(q => q.CustomerName is null)
            .Select(q => q.CustomerId)
            .Distinct()
            .ToList();
        var customersById = customerIds.Count == 0
            ? []
            : (await customers.GetByIdsAsync(customerIds, cancellationToken)).ToDictionary(c => c.Id);

        var recentQuotes = recent.Items
            .Select(q => new RecentQuoteDto(
                q.Id,
                q.Number,
                q.CustomerName ?? customersById.GetValueOrDefault(q.CustomerId)?.Name ?? string.Empty,
                q.Status,
                q.Breakdown.Total.Amount,
                q.Status == QuoteStatus.Draft,
                q.CreatedAt))
            .ToList();

        var activity = await auditEvents.GetRecentAsync(
            currentUser.IsAdministrator ? null : currentUser.UserId,
            RecentActivityCount,
            cancellationToken);

        return new DashboardDto(
            new DashboardIndicatorsDto(
                activeCustomers,
                activeProducts,
                portfolio.NumberedQuotes,
                portfolio.QuotedAmount.Amount),
            recentQuotes,
            activity
                .Select(e => new RecentActivityDto(
                    e.Id, e.OccurredAt, e.UserName, e.EventType, e.EntityType, e.EntityId, e.Detail))
                .ToList());
    }
}
