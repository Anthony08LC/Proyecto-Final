using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Security;

namespace QuoteFlow.Application.Assistant.Queries.GetAssistantStatus;

internal sealed class GetAssistantStatusQueryHandler(
    ITextAssistant assistant,
    AssistantUsageLimiter usageLimiter,
    ICurrentUser currentUser)
    : IQueryHandler<GetAssistantStatusQuery, AssistantStatusDto>
{
    public async Task<AssistantStatusDto> HandleAsync(
        GetAssistantStatusQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAuthenticated();

        if (!assistant.IsAvailable)
        {
            return new AssistantStatusDto(
                IsAvailable: false,
                AssistantUnavailableException.NotConfigured().Message,
                AssistantUsageLimiter.RequestsPerHour,
                RemainingThisHour: 0);
        }

        var remaining = await usageLimiter.GetRemainingAsync(currentUser.UserId, cancellationToken);
        return new AssistantStatusDto(true, null, AssistantUsageLimiter.RequestsPerHour, remaining);
    }
}
