using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application.Quotes.Commands.AcceptQuote;

internal sealed class AcceptQuoteCommandHandler(
    IQuoteRepository quotes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog,
    TimeProvider timeProvider)
    : ICommandHandler<AcceptQuoteCommand>
{
    public async Task HandleAsync(
        AcceptQuoteCommand command,
        CancellationToken cancellationToken = default)
    {
        var quote = await quotes.GetRequiredAsync(command.QuoteId, cancellationToken);
        currentUser.EnsureCanManage(quote);

        quote.Accept(timeProvider.GetUtcNow());

        auditLog.Record(new AuditEntry(
            QuoteAuditEvents.Accepted,
            AuditEntityType.Quote,
            quote.Id,
            $"Cotización {quote.DisplayNumber()} aceptada por el cliente."));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
