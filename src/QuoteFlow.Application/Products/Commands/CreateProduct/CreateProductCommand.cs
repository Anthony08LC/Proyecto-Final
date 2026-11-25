using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Products.Commands.CreateProduct;

/// <summary>
/// Crea un producto en el catálogo (RF-PRO-01). Solo Administrador. El nombre es único (RN-30),
/// la categoría debe estar activa (RN-29) y el precio es ≥ 0 con 2 decimales (RN-08).
/// Devuelve el id creado.
/// </summary>
public sealed record CreateProductCommand(
    string? Name,
    string? Description,
    Guid CategoryId,
    decimal Price) : ICommand<Guid>;
