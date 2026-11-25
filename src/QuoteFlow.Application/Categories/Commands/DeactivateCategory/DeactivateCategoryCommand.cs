using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Categories.Commands.DeactivateCategory;

/// <summary>
/// Desactiva una categoría (RF-CAT-04). Solo Administrador. No se podrá asignar a productos
/// nuevos ni editados (RN-29); los productos que ya la tienen no cambian.
/// </summary>
public sealed record DeactivateCategoryCommand(Guid CategoryId) : ICommand;
