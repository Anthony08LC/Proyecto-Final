using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Products.Common;

internal static class ProductExtensions
{
    public static ProductDto ToDto(this Product product, Category? category) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.CategoryId,
            category?.Name ?? string.Empty,
            category?.IsActive ?? false,
            product.Price.Amount,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt);

    public static async Task<Product> GetRequiredAsync(
        this IProductRepository repository,
        Guid productId,
        CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("el producto", productId);
    }

    /// <summary>
    /// Obtiene la categoría para asignarla a un producto: debe existir y estar activa (RN-29).
    /// </summary>
    public static async Task<Category> GetAssignableCategoryAsync(
        this ICategoryRepository repository,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new ValidationException("categoryId", "La categoría seleccionada no existe.");

        if (!category.IsActive)
        {
            throw new ValidationException(
                "categoryId",
                $"La categoría «{category.Name}» está inactiva y no se puede asignar.");
        }

        return category;
    }
}
