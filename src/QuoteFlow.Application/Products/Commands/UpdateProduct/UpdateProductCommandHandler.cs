using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Formatting;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Products.Common;

namespace QuoteFlow.Application.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(
    IProductRepository products,
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task HandleAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var product = await products.GetRequiredAsync(command.ProductId, cancellationToken);
        var input = ProductInput.Parse(command.Name, command.Description, command.CategoryId, command.Price);

        if (await products.NameExistsAsync(input.Name, product.Id, cancellationToken))
        {
            throw new ConflictException($"Ya existe otro producto con el nombre «{input.Name}».");
        }

        // Mantener la categoría actual también exige que esté activa (RN-29).
        var category = await categories.GetAssignableCategoryAsync(input.CategoryId, cancellationToken);

        var previousPrice = product.Price;

        product.Update(input.Name, input.Description, category, input.Price);

        auditLog.Record(new AuditEntry(
            ProductAuditEvents.Updated,
            AuditEntityType.Product,
            product.Id,
            $"Producto «{product.Name}» actualizado."));

        if (previousPrice != product.Price)
        {
            auditLog.Record(new AuditEntry(
                ProductAuditEvents.PriceChanged,
                AuditEntityType.Product,
                product.Id,
                $"Precio del producto «{product.Name}»: {DisplayFormat.Colones(previousPrice.Amount)} → {DisplayFormat.Colones(product.Price.Amount)}."));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
