using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Products.Commands.DeactivateProduct;

/// <summary>
/// Desactiva un producto (RF-PRO-04). Solo Administrador. Deja de aparecer en el editor,
/// bloquea la emisión de los borradores que lo contienen (RN-16) y no se copia al duplicar (RN-25).
/// </summary>
public sealed record DeactivateProductCommand(Guid ProductId) : ICommand;
