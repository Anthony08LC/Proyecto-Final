using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Quotes.Commands.UpdateQuoteTexts;

internal sealed class UpdateQuoteTextsCommandHandler(
    IQuoteRepository quotes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<UpdateQuoteTextsCommand>
{
    public async Task HandleAsync(
        UpdateQuoteTextsCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new ValidationErrors();
        var introduction = errors.Optional("introduction", "La introducción", command.Introduction, Quote.TextMaxLength);
        var executiveSummary = errors.Optional("executiveSummary", "El resumen ejecutivo", command.ExecutiveSummary, Quote.TextMaxLength);
        var notes = errors.Optional("notes", "Las observaciones", command.Notes, Quote.TextMaxLength);
        var commercialTerms = errors.Optional("commercialTerms", "Las condiciones comerciales", command.CommercialTerms, Quote.TextMaxLength);
        errors.ThrowIfAny();

        var quote = await quotes.GetRequiredAsync(command.QuoteId, cancellationToken);
        currentUser.EnsureCanManage(quote);

        quote.UpdateTexts(introduction, executiveSummary, notes, commercialTerms);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
