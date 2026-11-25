using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Settings.Common;

namespace QuoteFlow.Application.Settings.Queries.GetSystemSettings;

internal sealed class GetSystemSettingsQueryHandler(
    ISystemSettingsRepository settingsRepository,
    ICurrentUser currentUser)
    : IQueryHandler<GetSystemSettingsQuery, SystemSettingsDto>
{
    public async Task<SystemSettingsDto> HandleAsync(
        GetSystemSettingsQuery query,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var settings = await settingsRepository.GetAsync(cancellationToken);
        return settings.ToDto();
    }
}
