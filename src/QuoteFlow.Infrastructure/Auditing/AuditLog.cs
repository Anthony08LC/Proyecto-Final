using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Infrastructure.Persistence;
using QuoteFlow.Infrastructure.Persistence.Records;

namespace QuoteFlow.Infrastructure.Auditing;

/// <summary>
/// Registro de eventos de auditoría (RF-AUD-01). Los eventos quedan pendientes y
/// <see cref="Persistence.UnitOfWork"/> los inserta en la misma transacción que el cambio que
/// describen: si el cambio falla no queda ningún evento, y si el evento falla el cambio se deshace.
/// </summary>
internal sealed class AuditLog(TimeProvider timeProvider) : IAuditLog
{
    private readonly List<(AuditEntry Entry, DateTimeOffset OccurredAt)> _pending = [];

    public bool HasPending => _pending.Count > 0;

    public void Record(AuditEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _pending.Add((entry, timeProvider.GetUtcNow()));
    }

    /// <summary>
    /// Agrega los eventos pendientes al contexto. El usuario del evento es, en este orden: el indicado
    /// en la entrada, el usuario autenticado, ninguno para un intento de sesión anónimo (inicio de
    /// sesión fallido) o el usuario de sistema para los procesos automáticos.
    /// </summary>
    public async Task FlushAsync(
        ApplicationDbContext dbContext,
        AuditActorResolver actors,
        CancellationToken cancellationToken)
    {
        foreach (var (entry, occurredAt) in _pending)
        {
            var actor = await ResolveActorAsync(entry, actors, cancellationToken);

            dbContext.AuditEvents.Add(new AuditEvent
            {
                OccurredAt = occurredAt,
                UserId = actor?.UserId,
                UserName = Truncate(actor?.FullName, AuditEvent.UserNameMaxLength),
                EventType = entry.EventType,
                EntityType = entry.EntityType.ToString(),
                EntityId = entry.EntityId,
                Detail = Truncate(entry.Detail, AuditEvent.DetailMaxLength)!
            });
        }

        _pending.Clear();
    }

    private static async Task<AuditActor?> ResolveActorAsync(
        AuditEntry entry,
        AuditActorResolver actors,
        CancellationToken cancellationToken)
    {
        if (entry.ActorId is { } actorId)
        {
            return new AuditActor(actorId, entry.ActorName ?? string.Empty);
        }

        if (actors.GetAuthenticatedActor() is { } authenticated)
        {
            return authenticated;
        }

        return entry.EntityType == AuditEntityType.Session
            ? null
            : await actors.GetSystemUserAsync(cancellationToken);
    }

    private static string? Truncate(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";
}
