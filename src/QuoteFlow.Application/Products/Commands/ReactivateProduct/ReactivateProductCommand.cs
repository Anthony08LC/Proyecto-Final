using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Products.Commands.ReactivateProduct;

/// <summary>Reactiva un producto inactivo (RF-PRO-04). Solo Administrador.</summary>
public sealed record ReactivateProductCommand(Guid ProductId) : ICommand;
