using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Users.Commands.Login;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.ValueObjects;
using QuoteFlow.Infrastructure;
using QuoteFlow.Infrastructure.Persistence;

namespace QuoteFlow.Api.Tests.Infrastructure;

/// <summary>
/// API completa sobre PostgreSQL real (sección 8.4): el esquema de la base de
/// <c>ConnectionStrings__QuoteFlowTests</c> (que debe existir) se recrea con las mismas migraciones de producción y se cargan los datos iniciales más un
/// Administrador y un usuario de Ventas con contraseñas definitivas.
/// </summary>
public sealed class QuoteFlowApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ConnectionStringVariable = "ConnectionStrings__QuoteFlowTests";

    public const string AdminEmail = "admin@pruebas.quoteflow.local";
    public const string SalesEmail = "ventas@pruebas.quoteflow.local";
    public const string Password = "Pruebas-QuoteFlow-2026!";

    private readonly string connectionString = ReadConnectionString();
    private int nextClientAddress;

    public string AdminToken { get; private set; } = string.Empty;

    public string SalesToken { get; private set; } = string.Empty;

    private static string ReadConnectionString()
    {
        var value = Environment.GetEnvironmentVariable(ConnectionStringVariable);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Defina la variable de entorno {ConnectionStringVariable} con una base PostgreSQL exclusiva para pruebas.");
        }

        return value;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting($"ConnectionStrings:{DependencyInjection.ConnectionStringName}", connectionString);
        builder.UseSetting("Jwt:SigningKey", "clave-de-firma-solo-para-pruebas-de-integracion-0123456789");
        builder.UseSetting("Database:SeedOnStartup", "false");
        builder.UseSetting("Database:SeedDemoData", "false");
        builder.UseSetting("QuoteExpiration:Enabled", "false");
        builder.UseSetting("Gemini:ApiKey", string.Empty);

        // TestServer no asigna IP remota: cada solicitud recibe una distinta para que el limitador
        // de inicio de sesión (5 por minuto e IP) no interfiera entre pruebas.
        builder.ConfigureServices(services => services.AddSingleton<IStartupFilter>(
            new ClientAddressStartupFilter(() => Interlocked.Increment(ref nextClientAddress))));
    }

    public async Task InitializeAsync()
    {
        await using (var scope = Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            EnsureTestDatabase(db);

            // Se vacía el esquema en lugar de borrar la base: el usuario de pruebas solo necesita ser
            // dueño de la base, no el permiso CREATEDB.
            await db.Database.ExecuteSqlRawAsync(
                "DROP SCHEMA IF EXISTS public CASCADE; CREATE SCHEMA public;");
            await db.Database.MigrateAsync();
        }

        await Services.SeedDatabaseAsync();
        await CreateUsersAsync();

        AdminToken = await LoginAsync(AdminEmail, Password);
        SalesToken = await LoginAsync(SalesEmail, Password);
    }

    async Task IAsyncLifetime.DisposeAsync() => await base.DisposeAsync();

    /// <summary>Cliente HTTP con el token indicado (o anónimo si es nulo).</summary>
    public HttpClient CreateClient(string? token)
    {
        var client = CreateClient();

        if (token is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResult>(TestJson.Options);
        return result!.Token;
    }

    /// <summary>Evita borrar otra base: la API debe usar exactamente la cadena de pruebas.</summary>
    private void EnsureTestDatabase(ApplicationDbContext db)
    {
        var expected = new NpgsqlConnectionStringBuilder(connectionString);
        var actual = new NpgsqlConnectionStringBuilder(db.Database.GetConnectionString());

        if (!string.Equals(expected.Host, actual.Host, StringComparison.OrdinalIgnoreCase)
            || expected.Port != actual.Port
            || !string.Equals(expected.Database, actual.Database, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"La API no está usando la base de {ConnectionStringVariable}; no se recrea la base.");
        }
    }

    private async Task CreateUsersAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var systemUser = await db.Users.SingleAsync(u => u.IsSystem);
        var now = DateTimeOffset.UtcNow;

        foreach (var (name, email, role) in new[]
                 {
                     ("Admin de Pruebas", AdminEmail, UserRole.Administrator),
                     ("Ventas de Pruebas", SalesEmail, UserRole.Sales)
                 })
        {
            var user = User.Create(name, Email.Create(email), hasher.Hash(Password), role);
            user.ChangePassword(hasher.Hash(Password), isTemporary: false);
            user.MarkCreated(systemUser.Id, now);
            db.Users.Add(user);
        }

        await db.SaveChangesAsync();
    }

    private sealed class ClientAddressStartupFilter(Func<int> next) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> nextConfigure) => app =>
        {
            app.Use((context, nextMiddleware) =>
            {
                var n = next();
                context.Connection.RemoteIpAddress = new IPAddress([10, (byte)(n >> 16), (byte)(n >> 8), (byte)n]);
                return nextMiddleware(context);
            });
            nextConfigure(app);
        };
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<QuoteFlowApiFactory>
{
    public const string Name = "API";
}
