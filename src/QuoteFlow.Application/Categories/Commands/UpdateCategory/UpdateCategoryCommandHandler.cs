using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Categories.Common;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;

namespace QuoteFlow.Application.Categories.Commands.UpdateCategory;

internal sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task HandleAsync(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var category = await categories.GetRequiredAsync(command.CategoryId, cancellationToken);
        var input = CategoryInput.Parse(command.Name, command.Description);

        if (await categories.NameExistsAsync(input.Name, category.Id, cancellationToken))
        {
            throw new ConflictException($"Ya existe otra categoría con el nombre «{input.Name}».");
        }

        var previousName = category.Name;
        category.Update(input.Name, input.Description);

        var detail = previousName == category.Name
            ? $"Categoría «{category.Name}» actualizada."
            : $"Categoría «{previousName}» actualizada; nuevo nombre: «{category.Name}».";

        auditLog.Record(new AuditEntry(
            CategoryAuditEvents.Updated,
            AuditEntityType.Category,
            category.Id,
            detail));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
