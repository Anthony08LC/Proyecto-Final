using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Domain.Common;

/// <summary>
/// Clase base de todas las entidades del dominio. La identidad es un <see cref="Guid"/> (UUID v7)
/// y la igualdad se basa en el tipo y el identificador, no en los valores de sus atributos.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; private init; }

    /// <summary>Crea una nueva entidad con un identificador UUID v7 ordenado en el tiempo.</summary>
    protected Entity()
        : this(Guid.CreateVersion7())
    {
    }

    /// <summary>Reconstruye una entidad con un identificador existente.</summary>
    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("El identificador de la entidad no puede estar vacío.");
        }

        Id = id;
    }

    public bool Equals(Entity? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType() && Id == other.Id;
    }

    public override bool Equals(object? obj) => obj is Entity other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
