namespace QuoteFlow.Application.Products.Common;

/// <summary>Datos de un producto del catálogo, con su categoría.</summary>
public sealed record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    Guid CategoryId,
    string CategoryName,
    bool CategoryIsActive,
    decimal Price,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
