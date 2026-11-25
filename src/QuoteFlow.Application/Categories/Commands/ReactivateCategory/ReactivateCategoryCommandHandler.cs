using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Categories.Common;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;

namespace QuoteFlow.Application.Categories.Commands.ReactivateCategory;

internal sealed class ReactivateCategoryCommandHandler(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<ReactivateCategoryCommand>
{
    public async Task HandleAsync(
        ReactivateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var category = await categories.GetRequiredAsync(command.CategoryId, cancellationToken);

        if (category.IsActive)
        {
            throw new ConflictException("La categoría ya está activa.");
        }

        category.Reactivate();

        auditLog.Record(new AuditEntry(
            CategoryAuditEvents.Reactivated,
            AuditEntityType.Category,
            category.Id,
            $"Categoría «{category.Name}» reactivada."));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
