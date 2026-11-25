using Microsoft.EntityFrameworkCore;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Infrastructure.Persistence.Records;

namespace QuoteFlow.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core sobre PostgreSQL. Los nombres de tablas, columnas y restricciones siguen
/// la convención <c>snake_case</c> de la sección 9.1 de la especificación. La configuración de
/// cada entidad está en <c>Persistence/Configurations</c> (Fluent API).
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Quote> Quotes => Set<Quote>();

    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();

    internal DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    internal DbSet<QuoteNumberSequence> QuoteNumberSequences => Set<QuoteNumberSequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
