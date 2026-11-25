using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Categories.Common;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Categories.Commands.CreateCategory;

internal sealed class CreateCategoryCommandHandler(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var input = CategoryInput.Parse(command.Name, command.Description);

        var existing = await categories.GetByNameAsync(input.Name, cancellationToken);

        if (existing is not null)
        {
            throw new ConflictException(existing.IsActive
                ? $"Ya existe una categoría con el nombre «{existing.Name}»."
                : $"Ya existe una categoría inactiva con el nombre «{existing.Name}». Puede reactivarla en lugar de crear una nueva.");
        }

        var category = Category.Create(input.Name, input.Description);
        categories.Add(category);

        auditLog.Record(new AuditEntry(
            CategoryAuditEvents.Created,
            AuditEntityType.Category,
            category.Id,
            $"Categoría «{category.Name}» creada."));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
