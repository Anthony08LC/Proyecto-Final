using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Categories.Common;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;

namespace QuoteFlow.Application.Categories.Queries.GetCategories;

internal sealed class GetCategoriesQueryHandler(
    ICategoryRepository categories,
    IProductRepository products,
    ICurrentUser currentUser)
    : IQueryHandler<GetCategoriesQuery, PagedResult<CategoryDto>>
{
    public async Task<PagedResult<CategoryDto>> HandleAsync(
        GetCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var criteria = new CategorySearchCriteria(
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            query.IsActive);

        var page = await categories.SearchAsync(
            criteria,
            new PageRequest(query.Page, query.PageSize),
            cancellationToken);

        var counts = page.Items.Count == 0
            ? new Dictionary<Guid, int>()
            : await products.CountByCategoryAsync(page.Items.Select(c => c.Id), cancellationToken);

        return new PagedResult<CategoryDto>(
            page.Items.Select(c => c.ToDto(counts.GetValueOrDefault(c.Id))).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}
