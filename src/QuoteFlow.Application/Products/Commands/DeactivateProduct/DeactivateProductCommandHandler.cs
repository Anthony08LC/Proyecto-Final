using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Products.Common;

namespace QuoteFlow.Application.Products.Commands.DeactivateProduct;

internal sealed class DeactivateProductCommandHandler(
    IProductRepository products,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<DeactivateProductCommand>
{
    public async Task HandleAsync(
        DeactivateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var product = await products.GetRequiredAsync(command.ProductId, cancellationToken);

        if (!product.IsActive)
        {
            throw new ConflictException("El producto ya está inactivo.");
        }

        product.Deactivate();

        auditLog.Record(new AuditEntry(
            ProductAuditEvents.Deactivated,
            AuditEntityType.Product,
            product.Id,
            $"Producto «{product.Name}» desactivado."));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
