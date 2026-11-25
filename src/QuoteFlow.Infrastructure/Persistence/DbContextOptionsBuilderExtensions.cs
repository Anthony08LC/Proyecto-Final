using Microsoft.EntityFrameworkCore;

namespace QuoteFlow.Infrastructure.Persistence;

internal static class DbContextOptionsBuilderExtensions
{
    /// <summary>Tabla donde EF Core registra las migraciones aplicadas, en <c>snake_case</c> como el resto.</summary>
    public const string MigrationsHistoryTable = "__ef_migrations_history";

    /// <summary>
    /// Configuración única del proveedor, compartida por la aplicación y por las herramientas de
    /// EF Core (<see cref="DesignTimeDbContextFactory"/>), para que las migraciones se generen con
    /// exactamente el mismo modelo que se usa en ejecución.
    /// </summary>
    public static DbContextOptionsBuilder UseQuoteFlowDatabase(
        this DbContextOptionsBuilder builder,
        string connectionString)
    {
        return builder
            .UseNpgsql(connectionString, npgsql => npgsql
                .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                .MigrationsHistoryTable(MigrationsHistoryTable))
            .UseSnakeCaseNamingConvention();
    }
}
