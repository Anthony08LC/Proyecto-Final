using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Customers.Common;

namespace QuoteFlow.Application.Customers.Commands.RemoveCustomerVip;

internal sealed class RemoveCustomerVipCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<RemoveCustomerVipCommand>
{
    public async Task HandleAsync(
        RemoveCustomerVipCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator("Solo un Administrador puede cambiar la condición VIP de un cliente.");

        var customer = await customers.GetRequiredAsync(command.CustomerId, cancellationToken);

        if (!customer.IsVip)
        {
            throw new ConflictException("El cliente no es VIP.");
        }

        customer.RemoveVip();

        auditLog.Record(new AuditEntry(
            CustomerAuditEvents.VipChanged,
            AuditEntityType.Customer,
            customer.Id,
            $"Condición VIP del cliente «{customer.Name}»: Sí → No."));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
