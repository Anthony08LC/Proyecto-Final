using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Categories.Commands.CreateCategory;

/// <summary>Crea una categoría (RF-CAT-01). Solo Administrador. Devuelve el id creado.</summary>
public sealed record CreateCategoryCommand(string? Name, string? Description) : ICommand<Guid>;
