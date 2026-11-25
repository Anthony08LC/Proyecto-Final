using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Configuración del sistema: registro único creado por los datos iniciales (sección 9.7).
/// Se mantiene en memoria durante la solicitud para no consultarla varias veces.
/// </summary>
internal sealed class SystemSettingsRepository(ApplicationDbContext dbContext) : ISystemSettingsRepository
{
    private SystemSettings? _settings;

    public async Task<SystemSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        return _settings ??= await dbContext.SystemSettings.SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "La configuración del sistema no está inicializada. Ejecute la carga de datos iniciales.");
    }
}
