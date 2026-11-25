using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Customers.Common;

namespace QuoteFlow.Application.Customers.Commands.MarkCustomerAsVip;

internal sealed class MarkCustomerAsVipCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<MarkCustomerAsVipCommand>
{
    public async Task HandleAsync(
        MarkCustomerAsVipCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator("Solo un Administrador puede cambiar la condición VIP de un cliente.");

        var customer = await customers.GetRequiredAsync(command.CustomerId, cancellationToken);

        if (customer.IsVip)
        {
            throw new ConflictException("El cliente ya es VIP.");
        }

        customer.MarkAsVip();

        auditLog.Record(new AuditEntry(
            CustomerAuditEvents.VipChanged,
            AuditEntityType.Customer,
            customer.Id,
            $"Condición VIP del cliente «{customer.Name}»: No → Sí."));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
