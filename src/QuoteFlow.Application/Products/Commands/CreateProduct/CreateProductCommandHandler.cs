using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Formatting;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Products.Common;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IProductRepository products,
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var input = ProductInput.Parse(command.Name, command.Description, command.CategoryId, command.Price);

        if (await products.NameExistsAsync(input.Name, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"Ya existe un producto con el nombre «{input.Name}».");
        }

        var category = await categories.GetAssignableCategoryAsync(input.CategoryId, cancellationToken);

        var product = Product.Create(input.Name, input.Description, category, input.Price);
        products.Add(product);

        auditLog.Record(new AuditEntry(
            ProductAuditEvents.Created,
            AuditEntityType.Product,
            product.Id,
            $"Producto «{product.Name}» creado en la categoría «{category.Name}» con precio {DisplayFormat.Colones(product.Price.Amount)}."));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
