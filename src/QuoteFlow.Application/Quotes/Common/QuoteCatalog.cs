using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Quotes.Common;

/// <summary>Productos de las líneas de una cotización y sus categorías, con los datos vigentes.</summary>
internal sealed record QuoteCatalog(IReadOnlyList<Product> Products, IReadOnlyList<Category> Categories)
{
    public static readonly QuoteCatalog Empty = new([], []);

    public bool IsProductActive(Guid productId) =>
        Products.FirstOrDefault(p => p.Id == productId)?.IsActive ?? false;
}
