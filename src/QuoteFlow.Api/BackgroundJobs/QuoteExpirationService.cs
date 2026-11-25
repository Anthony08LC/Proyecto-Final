using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Quotes.Commands.ExpireQuotes;

namespace QuoteFlow.Api.BackgroundJobs;

/// <summary>Configuración del proceso de vencimiento (sección <c>QuoteExpiration</c>).</summary>
public sealed class QuoteExpirationOptions
{
    public const string SectionName = "QuoteExpiration";

    public bool Enabled { get; set; } = true;

    /// <summary>Frecuencia de ejecución. El proceso es idempotente, así que puede correr más de una vez al día.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>Espera inicial tras el arranque, para no competir con la inicialización.</summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// Vence automáticamente las cotizaciones emitidas cuya vigencia expiró (RF-COT-11). Corre al
/// arrancar y luego periódicamente, por lo que tras un reinicio vence todo lo pendiente
/// (RNF-DIS-02). Sin sesión HTTP, los cambios se atribuyen al usuario de sistema. Un fallo se
/// registra y se reintenta en la siguiente ejecución, sin detener la API.
/// </summary>
public sealed class QuoteExpirationService(
    IServiceScopeFactory scopeFactory,
    Microsoft.Extensions.Options.IOptions<QuoteExpirationOptions> options,
    ILogger<QuoteExpirationService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        if (!settings.Enabled)
        {
            logger.LogInformation("El proceso de vencimiento de cotizaciones está deshabilitado.");
            return;
        }

        try
        {
            await Task.Delay(settings.InitialDelay, stoppingToken);

            using var timer = new PeriodicTimer(settings.Interval);

            do
            {
                await RunOnceAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Apagado normal de la aplicación.
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ExpireQuotesCommand, int>>();

            var expired = await handler.HandleAsync(new ExpireQuotesCommand(), stoppingToken);

            logger.LogInformation("Proceso de vencimiento ejecutado: {ExpiredCount} cotizaciones vencidas.", expired);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Falló el proceso de vencimiento de cotizaciones; se reintentará en la siguiente ejecución.");
        }
    }
}
