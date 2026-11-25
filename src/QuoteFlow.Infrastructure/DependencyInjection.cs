using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Infrastructure.Assistant;
using QuoteFlow.Infrastructure.Auditing;
using QuoteFlow.Infrastructure.Documents;
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

            options.UseQuoteFlowDatabase(connectionString);
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

        // Documento comercial: vista previa HTML y PDF con QuestPDF (RF-DOC-01, RF-DOC-02).
        services.AddSingleton<IQuoteDocumentRenderer, QuoteDocumentRenderer>();

        // Asistente de redacción con Gemini (sección 11): sin clave, la versión «no disponible» (RF-IA-03).
        AddTextAssistant(services, configuration);

        // Datos iniciales.
        services.AddOptions<SeedOptions>().Bind(configuration.GetSection(SeedOptions.SectionName));
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<DemoDataSeeder>();

        return services;
    }

    private static void AddTextAssistant(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(GeminiOptions.SectionName);
        services.AddOptions<GeminiOptions>().Bind(section);

        var gemini = section.Get<GeminiOptions>() ?? new GeminiOptions();
        if (!gemini.IsConfigured)
        {
            services.AddSingleton<ITextAssistant, UnavailableTextAssistant>();
            return;
        }

        services.AddHttpClient<ITextAssistant, GeminiTextAssistant>(client =>
        {
            client.BaseAddress = new Uri(gemini.Endpoint.EndsWith('/') ? gemini.Endpoint : gemini.Endpoint + "/");
            // El tiempo límite de 20 s lo aplica el asistente; este es solo un respaldo.
            client.Timeout = gemini.Timeout + TimeSpan.FromSeconds(10);
        });
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

    /// <summary>
    /// Carga los datos de demostración (sección 8.3, T-090). Solo en desarrollo: el seeder verifica el
    /// entorno y se niega en cualquier otro. Debe ejecutarse después de <see cref="SeedDatabaseAsync"/>.
    /// </summary>
    public static async Task SeedDemoDataAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
