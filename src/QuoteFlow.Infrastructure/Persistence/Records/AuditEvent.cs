namespace QuoteFlow.Infrastructure.Persistence.Records;

/// <summary>
/// Fila de <c>audit_events</c>: evento inmutable, solo inserción y lectura (RF-AUD-01).
/// No es una entidad del dominio; la aplicación lo registra mediante <c>IAuditLog</c>.
/// </summary>
internal sealed class AuditEvent
{
    public const int EventTypeMaxLength = 50;
    public const int EntityTypeMaxLength = 30;
    public const int UserNameMaxLength = 150;
    public const int DetailMaxLength = 2000;

    public long Id { get; private set; }

    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>Nulo solo en un inicio de sesión fallido.</summary>
    public Guid? UserId { get; init; }

    /// <summary>Copia del nombre del usuario al momento del evento.</summary>
    public string? UserName { get; init; }

    public required string EventType { get; init; }

    public required string EntityType { get; init; }

    /// <summary>Sin llave foránea; nulo para la configuración y las sesiones.</summary>
    public Guid? EntityId { get; init; }

    public required string Detail { get; init; }
}
