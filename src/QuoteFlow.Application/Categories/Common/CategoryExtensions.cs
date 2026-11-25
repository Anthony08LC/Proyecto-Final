using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Categories.Common;

internal static class CategoryExtensions
{
    public static CategoryDto ToDto(this Category category, int productCount) =>
        new(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            productCount,
            category.CreatedAt,
            category.UpdatedAt);

    public static async Task<Category> GetRequiredAsync(
        this ICategoryRepository repository,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException("la categoría", categoryId);
    }
}
