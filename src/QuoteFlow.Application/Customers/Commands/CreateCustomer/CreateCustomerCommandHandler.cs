using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Customers.Common;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Customers.Commands.CreateCustomer;

internal sealed class CreateCustomerCommandHandler(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<CreateCustomerCommand, CustomerSaveResult>
{
    public async Task<CustomerSaveResult> HandleAsync(
        CreateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        if (command.IsVip)
        {
            currentUser.EnsureAdministrator("Solo un Administrador puede registrar un cliente como VIP.");
        }

        var input = CustomerInput.Parse(
            command.Name,
            command.Company,
            command.Email,
            command.Phone,
            command.Notes);

        var possibleDuplicate = input.Email is not null
            && await customers.EmailInUseAsync(input.Email, cancellationToken: cancellationToken);

        var customer = Customer.Create(
            input.Name,
            input.Company,
            input.Email,
            input.Phone,
            input.Notes,
            command.IsVip);

        customers.Add(customer);

        auditLog.Record(new AuditEntry(
            CustomerAuditEvents.Created,
            AuditEntityType.Customer,
            customer.Id,
            $"Cliente «{customer.Name}» registrado{(customer.IsVip ? " como VIP" : string.Empty)}."));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CustomerSaveResult(customer.Id, possibleDuplicate);
    }
}
