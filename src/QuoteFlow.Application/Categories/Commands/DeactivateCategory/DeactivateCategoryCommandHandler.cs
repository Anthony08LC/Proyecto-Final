using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Categories.Common;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;

namespace QuoteFlow.Application.Categories.Commands.DeactivateCategory;

internal sealed class DeactivateCategoryCommandHandler(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<DeactivateCategoryCommand>
{
    public async Task HandleAsync(
        DeactivateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var category = await categories.GetRequiredAsync(command.CategoryId, cancellationToken);

        if (!category.IsActive)
        {
            throw new ConflictException("La categoría ya está inactiva.");
        }

        category.Deactivate();

        auditLog.Record(new AuditEntry(
            CategoryAuditEvents.Deactivated,
            AuditEntityType.Category,
            category.Id,
            $"Categoría «{category.Name}» desactivada."));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
