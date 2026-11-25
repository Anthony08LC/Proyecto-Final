using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace QuoteFlow.Infrastructure.Persistence;

/// <summary>
/// Crea el <see cref="ApplicationDbContext"/> para las herramientas de EF Core
/// (<c>dotnet ef migrations add</c>, <c>dotnet ef database update</c>) sin arrancar la API, que exige
/// la clave del JWT y carga los datos iniciales al iniciar.
/// <para>
/// La cadena de conexión se toma, en orden de prioridad, de: el argumento
/// <c>--connection "…"</c>, la variable de entorno <c>ConnectionStrings__QuoteFlow</c>, o
/// <c>appsettings.json</c>/<c>appsettings.{Entorno}.json</c> del proyecto de inicio. Generar una
/// migración no se conecta a la base de datos, así que sin configuración se usa una cadena local.
/// </para>
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string LocalFallback = "Host=localhost;Port=5432;Database=quoteflow;Username=postgres";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(DependencyInjection.ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine(
                $"[QuoteFlow] No se encontró ConnectionStrings__{DependencyInjection.ConnectionStringName}; " +
                $"se usa «{LocalFallback}». Para aplicar migraciones defina la variable o use --connection.");
            connectionString = LocalFallback;
        }

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        builder.UseQuoteFlowDatabase(connectionString);

        return new ApplicationDbContext(builder.Options);
    }
}
