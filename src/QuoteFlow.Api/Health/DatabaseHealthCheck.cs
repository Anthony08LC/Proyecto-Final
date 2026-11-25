using Microsoft.Extensions.Diagnostics.HealthChecks;
using QuoteFlow.Infrastructure.Persistence;

namespace QuoteFlow.Api.Health;

/// <summary>Verifica que la base de datos PostgreSQL responda.</summary>
public sealed class DatabaseHealthCheck(ApplicationDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("La base de datos responde.")
                : HealthCheckResult.Unhealthy("No se puede conectar con la base de datos.");
        }
        catch (Exception ex)
        {
            // Sin detalles en la respuesta: el error queda en los logs del health check.
            return HealthCheckResult.Unhealthy("No se puede conectar con la base de datos.", ex);
        }
    }
}
