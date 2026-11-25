using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Categories.Common;
using QuoteFlow.Application.Common.Security;

namespace QuoteFlow.Application.Categories.Queries.GetCategoryById;

internal sealed class GetCategoryByIdQueryHandler(
    ICategoryRepository categories,
    IProductRepository products,
    ICurrentUser currentUser)
    : IQueryHandler<GetCategoryByIdQuery, CategoryDto>
{
    public async Task<CategoryDto> HandleAsync(
        GetCategoryByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var category = await categories.GetRequiredAsync(query.CategoryId, cancellationToken);
        var counts = await products.CountByCategoryAsync([category.Id], cancellationToken);

        return category.ToDto(counts.GetValueOrDefault(category.Id));
    }
}
