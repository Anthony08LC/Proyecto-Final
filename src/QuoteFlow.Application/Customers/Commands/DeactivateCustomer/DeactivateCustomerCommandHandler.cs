using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Customers.Common;

namespace QuoteFlow.Application.Customers.Commands.DeactivateCustomer;

internal sealed class DeactivateCustomerCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<DeactivateCustomerCommand>
{
    public async Task HandleAsync(
        DeactivateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var customer = await customers.GetRequiredAsync(command.CustomerId, cancellationToken);

        if (!customer.IsActive)
        {
            throw new ConflictException("El cliente ya está inactivo.");
        }

        customer.Deactivate();

        auditLog.Record(new AuditEntry(
            CustomerAuditEvents.Deactivated,
            AuditEntityType.Customer,
            customer.Id,
            $"Cliente «{customer.Name}» desactivado."));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
