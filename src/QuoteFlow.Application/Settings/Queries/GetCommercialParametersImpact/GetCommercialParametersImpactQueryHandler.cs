using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Settings.Common;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Settings.Queries.GetCommercialParametersImpact;

internal sealed class GetCommercialParametersImpactQueryHandler(
    IQuoteRepository quotes,
    ICurrentUser currentUser)
    : IQueryHandler<GetCommercialParametersImpactQuery, CommercialParametersImpactDto>
{
    public async Task<CommercialParametersImpactDto> HandleAsync(
        GetCommercialParametersImpactQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var drafts = await quotes.CountByStatusAsync(QuoteStatus.Draft, cancellationToken);
        return new CommercialParametersImpactDto(drafts);
    }
}
