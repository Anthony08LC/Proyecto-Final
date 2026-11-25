using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Products.Common;

namespace QuoteFlow.Application.Products.Queries.GetProductById;

/// <summary>Obtiene el detalle de un producto.</summary>
public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDto>;
