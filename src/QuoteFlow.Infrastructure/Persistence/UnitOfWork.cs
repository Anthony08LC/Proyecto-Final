using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Domain.Common;
using QuoteFlow.Infrastructure.Auditing;

namespace QuoteFlow.Infrastructure.Persistence;

/// <summary>
/// Confirma en una sola transacción los cambios de los agregados y los eventos de auditoría
/// (RN-19). Antes de guardar completa los campos de auditoría (sección 9.2) y traduce los
/// conflictos de concurrencia y de unicidad a <see cref="ConflictException"/> (RNF-DAT-04).
/// </summary>
internal sealed class UnitOfWork(
    ApplicationDbContext dbContext,
    AuditLog auditLog,
    AuditActorResolver actors,
    TimeProvider timeProvider) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await StampAuditFieldsAsync(cancellationToken);
        await auditLog.FlushAsync(dbContext, actors, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "Otro usuario modificó estos datos mientras los editaba. Vuelva a cargarlos e intente de nuevo.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        })
        {
            throw new ConflictException("Ya existe un registro con esos datos.");
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (dbContext.Database.CurrentTransaction is not null)
        {
            return await operation(cancellationToken);
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            operation,
            async (_, op, ct) =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
                var result = await op(ct);
                await transaction.CommitAsync(ct);
                return result;
            },
            verifySucceeded: null,
            cancellationToken);
    }

    /// <summary>
    /// Registra quién y cuándo creó o modificó cada raíz de agregado. Una línea agregada o
    /// quitada también cuenta como modificación de su cotización.
    /// </summary>
    private async Task StampAuditFieldsAsync(CancellationToken cancellationToken)
    {
        dbContext.ChangeTracker.DetectChanges();

        var entries = dbContext.ChangeTracker
            .Entries<AuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified
                || (e.State == EntityState.Unchanged && HasChangedChildren(e.Entity)))
            .ToList();

        if (entries.Count == 0)
        {
            return;
        }

        var actor = await actors.GetActorAsync(cancellationToken);
        var now = timeProvider.GetUtcNow();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.MarkCreated(actor.UserId, now);
            }
            else
            {
                entry.Entity.MarkUpdated(actor.UserId, now);
            }
        }
    }

    private bool HasChangedChildren(AuditableEntity entity) =>
        entity is Domain.Entities.Quote quote
        && dbContext.ChangeTracker
            .Entries<Domain.Entities.QuoteLine>()
            .Any(line => line.State != EntityState.Unchanged
                && (Guid?)line.Property(Configurations.QuoteLineConfiguration.QuoteIdProperty).CurrentValue == quote.Id);
}
