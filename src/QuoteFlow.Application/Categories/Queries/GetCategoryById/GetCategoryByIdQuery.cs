using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Categories.Common;

namespace QuoteFlow.Application.Categories.Queries.GetCategoryById;

/// <summary>Obtiene el detalle de una categoría (para su edición).</summary>
public sealed record GetCategoryByIdQuery(Guid CategoryId) : IQuery<CategoryDto>;
