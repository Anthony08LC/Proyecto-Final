using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Products.Common;

namespace QuoteFlow.Application.Products.Commands.ReactivateProduct;

internal sealed class ReactivateProductCommandHandler(
    IProductRepository products,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<ReactivateProductCommand>
{
    public async Task HandleAsync(
        ReactivateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var product = await products.GetRequiredAsync(command.ProductId, cancellationToken);

        if (product.IsActive)
        {
            throw new ConflictException("El producto ya está activo.");
        }

        product.Reactivate();

        auditLog.Record(new AuditEntry(
            ProductAuditEvents.Reactivated,
            AuditEntityType.Product,
            product.Id,
            $"Producto «{product.Name}» reactivado."));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
