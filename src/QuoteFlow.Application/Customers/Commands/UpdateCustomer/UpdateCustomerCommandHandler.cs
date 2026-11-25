using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Customers.Common;

namespace QuoteFlow.Application.Customers.Commands.UpdateCustomer;

internal sealed class UpdateCustomerCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<UpdateCustomerCommand, CustomerSaveResult>
{
    public async Task<CustomerSaveResult> HandleAsync(
        UpdateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var customer = await customers.GetRequiredAsync(command.CustomerId, cancellationToken);

        var input = CustomerInput.Parse(
            command.Name,
            command.Company,
            command.Email,
            command.Phone,
            command.Notes);

        var possibleDuplicate = input.Email is not null
            && await customers.EmailInUseAsync(input.Email, customer.Id, cancellationToken);

        customer.Update(input.Name, input.Company, input.Email, input.Phone, input.Notes);

        auditLog.Record(new AuditEntry(
            CustomerAuditEvents.Updated,
            AuditEntityType.Customer,
            customer.Id,
            $"Datos del cliente «{customer.Name}» actualizados."));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CustomerSaveResult(customer.Id, possibleDuplicate);
    }
}
