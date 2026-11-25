using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Formatting;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Common;

namespace QuoteFlow.Application.Quotes.Commands.ExpireQuotes;

internal sealed class ExpireQuotesCommandHandler(
    IQuoteRepository quotes,
    IUnitOfWork unitOfWork,
    IAuditLog auditLog,
    TimeProvider timeProvider)
    : ICommandHandler<ExpireQuotesCommand, int>
{
    public async Task<int> HandleAsync(
        ExpireQuotesCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var today = CostaRicaTime.ToLocalDate(now);

        var expired = await quotes.GetIssuedExpiredBeforeAsync(today, cancellationToken);

        if (expired.Count == 0)
        {
            return 0;
        }

        foreach (var quote in expired)
        {
            quote.Expire(now);

            auditLog.Record(new AuditEntry(
                QuoteAuditEvents.Expired,
                AuditEntityType.Quote,
                quote.Id,
                $"Cotización {quote.DisplayNumber()} vencida (vigente hasta {DisplayFormat.Date(quote.ValidUntil!.Value)})."));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return expired.Count;
    }
}
