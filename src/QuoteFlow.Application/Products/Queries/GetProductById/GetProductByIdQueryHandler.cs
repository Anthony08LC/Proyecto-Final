using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Products.Common;

namespace QuoteFlow.Application.Products.Queries.GetProductById;

internal sealed class GetProductByIdQueryHandler(
    IProductRepository products,
    ICategoryRepository categories,
    ICurrentUser currentUser)
    : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> HandleAsync(
        GetProductByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var product = await products.GetRequiredAsync(query.ProductId, cancellationToken);
        var category = await categories.GetByIdAsync(product.CategoryId, cancellationToken);

        return product.ToDto(category);
    }
}
