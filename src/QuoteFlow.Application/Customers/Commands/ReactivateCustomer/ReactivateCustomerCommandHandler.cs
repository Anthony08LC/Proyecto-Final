using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Customers.Common;

namespace QuoteFlow.Application.Customers.Commands.ReactivateCustomer;

internal sealed class ReactivateCustomerCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<ReactivateCustomerCommand>
{
    public async Task HandleAsync(
        ReactivateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var customer = await customers.GetRequiredAsync(command.CustomerId, cancellationToken);

        if (customer.IsActive)
        {
            throw new ConflictException("El cliente ya está activo.");
        }

        customer.Reactivate();

        auditLog.Record(new AuditEntry(
            CustomerAuditEvents.Reactivated,
            AuditEntityType.Customer,
            customer.Id,
            $"Cliente «{customer.Name}» reactivado."));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
