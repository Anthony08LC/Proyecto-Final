using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Quotes.Commands.CancelQuote;

internal sealed class CancelQuoteCommandHandler(
    IQuoteRepository quotes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog,
    TimeProvider timeProvider)
    : ICommandHandler<CancelQuoteCommand>
{
    public async Task HandleAsync(
        CancelQuoteCommand command,
        CancellationToken cancellationToken = default)
    {
        var quote = await quotes.GetRequiredAsync(command.QuoteId, cancellationToken);
        currentUser.EnsureCanManage(quote);

        var errors = new ValidationErrors();
        var reason = quote.Status == QuoteStatus.Issued
            ? errors.Required("reason", "El motivo", command.Reason, Quote.CloseReasonMaxLength)
            : errors.Optional("reason", "El motivo", command.Reason, Quote.CloseReasonMaxLength);
        errors.ThrowIfAny();

        quote.Cancel(timeProvider.GetUtcNow(), reason);

        auditLog.Record(new AuditEntry(
            QuoteAuditEvents.Cancelled,
            AuditEntityType.Quote,
            quote.Id,
            reason is null
                ? $"Cotización {quote.DisplayNumber()} cancelada."
                : $"Cotización {quote.DisplayNumber()} cancelada. Motivo: {reason}"));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
