using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Quotes.Commands.RejectQuote;

internal sealed class RejectQuoteCommandHandler(
    IQuoteRepository quotes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog,
    TimeProvider timeProvider)
    : ICommandHandler<RejectQuoteCommand>
{
    public async Task HandleAsync(
        RejectQuoteCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new ValidationErrors();
        var reason = errors.Optional("reason", "El motivo", command.Reason, Quote.CloseReasonMaxLength);
        errors.ThrowIfAny();

        var quote = await quotes.GetRequiredAsync(command.QuoteId, cancellationToken);
        currentUser.EnsureCanManage(quote);

        quote.Reject(timeProvider.GetUtcNow(), reason);

        auditLog.Record(new AuditEntry(
            QuoteAuditEvents.Rejected,
            AuditEntityType.Quote,
            quote.Id,
            reason is null
                ? $"Cotización {quote.DisplayNumber()} rechazada por el cliente."
                : $"Cotización {quote.DisplayNumber()} rechazada por el cliente. Motivo: {reason}"));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
