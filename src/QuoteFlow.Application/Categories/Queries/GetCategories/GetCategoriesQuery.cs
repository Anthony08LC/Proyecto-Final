using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Categories.Common;
using QuoteFlow.Application.Common.Models;

namespace QuoteFlow.Application.Categories.Queries.GetCategories;

/// <summary>
/// Lista las categorías con su estado y cantidad de productos, filtradas por nombre y estado
/// (RF-CAT-02). Disponible para todos los usuarios, porque sirve de filtro del catálogo.
/// </summary>
public sealed record GetCategoriesQuery(
    string? Search = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize) : IQuery<PagedResult<CategoryDto>>;
