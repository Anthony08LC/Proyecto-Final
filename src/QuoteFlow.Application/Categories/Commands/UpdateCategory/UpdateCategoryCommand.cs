using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Categories.Commands.UpdateCategory;

/// <summary>
/// Edita una categoría (RF-CAT-03). Solo Administrador. Las líneas de las cotizaciones
/// emitidas conservan el nombre congelado (RN-12).
/// </summary>
public sealed record UpdateCategoryCommand(Guid CategoryId, string? Name, string? Description) : ICommand;
