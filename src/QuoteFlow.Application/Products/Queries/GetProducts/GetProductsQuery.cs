using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Products.Common;

namespace QuoteFlow.Application.Products.Queries.GetProducts;

/// <summary>
/// Busca productos por nombre, con filtros por categoría y estado. Resultado paginado
/// y ordenado por nombre (RF-PRO-02). El editor de cotizaciones usa <c>IsActive = true</c>.
/// </summary>
public sealed record GetProductsQuery(
    string? Search = null,
    Guid? CategoryId = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize) : IQuery<PagedResult<ProductDto>>;
