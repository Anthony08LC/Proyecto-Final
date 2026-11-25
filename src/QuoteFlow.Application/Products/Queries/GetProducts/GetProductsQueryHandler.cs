using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Products.Common;

namespace QuoteFlow.Application.Products.Queries.GetProducts;

internal sealed class GetProductsQueryHandler(
    IProductRepository products,
    ICategoryRepository categories,
    ICurrentUser currentUser)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> HandleAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var criteria = new ProductSearchCriteria(
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            query.CategoryId,
            query.IsActive);

        var page = await products.SearchAsync(
            criteria,
            new PageRequest(query.Page, query.PageSize),
            cancellationToken);

        var categoryIds = page.Items.Select(p => p.CategoryId).Distinct().ToList();
        var categoriesById = categoryIds.Count == 0
            ? []
            : (await categories.GetByIdsAsync(categoryIds, cancellationToken)).ToDictionary(c => c.Id);

        return new PagedResult<ProductDto>(
            page.Items.Select(p => p.ToDto(categoriesById.GetValueOrDefault(p.CategoryId))).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}
