using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Domain.Common;

/// <summary>
/// Entidad que registra quién la creó y quién la modificó por última vez, y cuándo.
/// Los valores solo se asignan mediante <see cref="MarkCreated"/> y <see cref="MarkUpdated"/>,
/// que la capa de persistencia invoca automáticamente con el usuario autenticado (o el de sistema).
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Guid UpdatedBy { get; private set; }

    protected AuditableEntity()
    {
    }

    protected AuditableEntity(Guid id)
        : base(id)
    {
    }

    /// <summary>
    /// Registra los datos de auditoría de creación. Al crear, los campos de actualización
    /// son iguales a los de creación.
    /// </summary>
    public void MarkCreated(Guid userId, DateTimeOffset at)
    {
        EnsureValidUser(userId);

        if (CreatedBy != Guid.Empty)
        {
            throw new DomainException("Los datos de auditoría de creación ya fueron registrados.");
        }

        CreatedAt = at;
        CreatedBy = userId;
        UpdatedAt = at;
        UpdatedBy = userId;
    }

    /// <summary>Registra los datos de auditoría de la última modificación.</summary>
    public void MarkUpdated(Guid userId, DateTimeOffset at)
    {
        EnsureValidUser(userId);

        if (CreatedBy == Guid.Empty)
        {
            throw new DomainException(
                "No se puede registrar una actualización antes de registrar los datos de creación.");
        }

        if (at < CreatedAt)
        {
            throw new DomainException(
                "La fecha de actualización no puede ser anterior a la fecha de creación.");
        }

        UpdatedAt = at;
        UpdatedBy = userId;
    }

    private static void EnsureValidUser(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("El identificador del usuario de auditoría no puede estar vacío.");
        }
    }
}
