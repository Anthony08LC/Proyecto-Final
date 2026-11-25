namespace QuoteFlow.Application.Categories.Common;

/// <summary>Datos de una categoría, con la cantidad de productos que la usan (RF-CAT-02).</summary>
public sealed record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int ProductCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
