using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Quotes.Commands.CreateQuote;

internal sealed class CreateQuoteCommandHandler(
    IQuoteRepository quotes,
    ICustomerRepository customers,
    ISystemSettingsRepository settingsRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<CreateQuoteCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        CreateQuoteCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        var customer = await customers.GetByIdAsync(command.CustomerId, cancellationToken)
            ?? throw new ValidationException("customerId", "El cliente seleccionado no existe.");

        if (!customer.IsActive)
        {
            throw new ValidationException(
                "customerId",
                $"El cliente «{customer.Name}» está inactivo; no se pueden crear cotizaciones para él.");
        }

        var settings = await settingsRepository.GetAsync(cancellationToken);

        var quote = Quote.Create(customer, currentUser.UserId, settings.DefaultCommercialTerms);
        quote.Recalculate(customer, settings);
        quotes.Add(quote);

        auditLog.Record(new AuditEntry(
            QuoteAuditEvents.Created,
            AuditEntityType.Quote,
            quote.Id,
            $"Borrador creado para el cliente «{customer.Name}»."));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return quote.Id;
    }
}
