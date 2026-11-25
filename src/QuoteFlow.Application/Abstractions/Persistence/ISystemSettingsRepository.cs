using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Abstractions.Persistence;

/// <summary>Acceso a la configuración del sistema, que es un registro único.</summary>
public interface ISystemSettingsRepository
{
    Task<SystemSettings> GetAsync(CancellationToken cancellationToken = default);
}
