using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Infrastructure.Persistence.Seeding;

/// <summary>
/// Credenciales del primer Administrador (sección <c>Seed</c>), provenientes de variables de entorno
/// como <c>Seed__AdminEmail</c> y <c>Seed__AdminPassword</c> (RNF-SEG-07).
/// </summary>
public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminFullName { get; set; } = "Administrador";

    public string? AdminEmail { get; set; }

    public string? AdminPassword { get; set; }
}

/// <summary>
/// Crea los datos iniciales sin intervención manual (sección 9.7, RNF-OPE-02): usuario de sistema,
/// primer Administrador con contraseña temporal, categorías base y configuración por defecto.
/// Es idempotente: solo crea lo que falta.
/// </summary>
public sealed class DatabaseSeeder(
    ApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IOptions<SeedOptions> options,
    TimeProvider timeProvider,
    ILogger<DatabaseSeeder> logger)
{
    public const string SystemUserEmail = "system@quoteflow.local";
    public const string SystemUserName = "Sistema";

    private static readonly string[] InitialCategories =
    [
        "Equipo tecnológico",
        "Licencia de software",
        "Servicio profesional",
        "Solución empresarial"
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var systemUser = await EnsureSystemUserAsync(now, cancellationToken);

        await EnsureFirstAdministratorAsync(systemUser.Id, now, cancellationToken);
        await EnsureCategoriesAsync(systemUser.Id, now, cancellationToken);
        await EnsureSettingsAsync(systemUser.Id, now, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> EnsureSystemUserAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var systemUser = await dbContext.Users.SingleOrDefaultAsync(u => u.IsSystem, cancellationToken);

        if (systemUser is not null)
        {
            return systemUser;
        }

        systemUser = User.CreateSystem(SystemUserName, Email.Create(SystemUserEmail));
        systemUser.MarkCreated(systemUser.Id, now);
        dbContext.Users.Add(systemUser);

        // Se guarda primero: el resto de los datos iniciales lo referencia como creador.
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Usuario de sistema creado.");

        return systemUser;
    }

    private async Task EnsureFirstAdministratorAsync(
        Guid systemUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var hasAdministrator = await dbContext.Users
            .AnyAsync(u => !u.IsSystem && u.Role == UserRole.Administrator, cancellationToken);

        if (hasAdministrator)
        {
            return;
        }

        var seed = options.Value;

        if (string.IsNullOrWhiteSpace(seed.AdminEmail) || string.IsNullOrWhiteSpace(seed.AdminPassword))
        {
            logger.LogWarning(
                "No existe un Administrador y faltan {EmailVariable} o {PasswordVariable}: no se creó el primer Administrador.",
                $"{SeedOptions.SectionName}__{nameof(SeedOptions.AdminEmail)}",
                $"{SeedOptions.SectionName}__{nameof(SeedOptions.AdminPassword)}");
            return;
        }

        // Contraseña temporal: deberá cambiarla en su primer inicio de sesión (RF-AUT-07).
        var administrator = User.Create(
            seed.AdminFullName,
            Email.Create(seed.AdminEmail),
            passwordHasher.Hash(seed.AdminPassword),
            UserRole.Administrator);

        administrator.MarkCreated(systemUserId, now);
        dbContext.Users.Add(administrator);
        logger.LogInformation("Primer Administrador creado.");
    }

    private async Task EnsureCategoriesAsync(
        Guid systemUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var name in InitialCategories)
        {
            var category = Category.Create(name, description: null);
            category.MarkCreated(systemUserId, now);
            dbContext.Categories.Add(category);
        }
    }

    private async Task EnsureSettingsAsync(
        Guid systemUserId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (await dbContext.SystemSettings.AnyAsync(cancellationToken))
        {
            return;
        }

        var settings = Domain.Entities.SystemSettings.CreateDefault();
        settings.MarkCreated(systemUserId, now);
        dbContext.SystemSettings.Add(settings);
    }
}
