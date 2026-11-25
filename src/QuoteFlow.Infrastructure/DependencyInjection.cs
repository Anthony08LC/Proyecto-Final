using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Infrastructure.Auditing;
using QuoteFlow.Infrastructure.Persistence;
using QuoteFlow.Infrastructure.Persistence.Repositories;
using QuoteFlow.Infrastructure.Persistence.Seeding;
using QuoteFlow.Infrastructure.Security;

namespace QuoteFlow.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Nombre de la cadena de conexión (variable de entorno <c>ConnectionStrings__QuoteFlow</c>).</summary>
    public const string ConnectionStringName = "QuoteFlow";

    /// <summary>
    /// Registra la persistencia en PostgreSQL y las implementaciones de todas las interfaces de la
    /// capa de aplicación. La cadena de conexión y la clave del JWT se leen al usarse por primera vez,
    /// por lo que su ausencia se informa con un error claro en ese momento.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString(ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Falta la cadena de conexión «{ConnectionStringName}» (variable de entorno ConnectionStrings__{ConnectionStringName}).");
            }

            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
        });

        // Persistencia.
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<ISystemSettingsRepository, SystemSettingsRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IQuoteNumberGenerator, QuoteNumberGenerator>();

        // Auditoría: el mismo registro por solicitud para IAuditLog y la unidad de trabajo.
        services.AddScoped<AuditLog>();
        services.AddScoped<IAuditLog>(provider => provider.GetRequiredService<AuditLog>());
        services.AddScoped<IAuditEventReader, AuditEventReader>();
        services.AddScoped<AuditActorResolver>();

        // Seguridad.
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
        services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.SectionName));

        // Datos iniciales.
        services.AddOptions<SeedOptions>().Bind(configuration.GetSection(SeedOptions.SectionName));
        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    /// <summary>
    /// Crea los datos iniciales (sección 9.7). Se invoca al arrancar la API, una vez aplicadas las migraciones.
    /// </summary>
    public static async Task SeedDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
