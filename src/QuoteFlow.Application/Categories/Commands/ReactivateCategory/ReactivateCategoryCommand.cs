using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Categories.Commands.ReactivateCategory;

/// <summary>Reactiva una categoría inactiva (RF-CAT-04). Solo Administrador.</summary>
public sealed record ReactivateCategoryCommand(Guid CategoryId) : ICommand;
