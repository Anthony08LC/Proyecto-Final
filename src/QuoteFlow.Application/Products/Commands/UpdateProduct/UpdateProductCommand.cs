using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Products.Commands.UpdateProduct;

/// <summary>
/// Edita un producto, incluido su precio (RF-PRO-03). Solo Administrador. Un cambio de precio
/// registra un evento específico con el precio anterior y el nuevo. Afecta a los borradores
/// (RN-22) y nunca a las cotizaciones emitidas (RN-12).
/// </summary>
public sealed record UpdateProductCommand(
    Guid ProductId,
    string? Name,
    string? Description,
    Guid CategoryId,
    decimal Price) : ICommand;
