using QuoteFlow.Domain.Common;
using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Domain.Entities;

/// <summary>
/// Categoría del catálogo de productos. No se elimina físicamente: se desactiva (RN-20).
/// La unicidad del nombre (RN-30) se valida en la capa de aplicación.
/// </summary>
public sealed class Category : AuditableEntity
{
    public const int NameMaxLength = 80;
    public const int DescriptionMaxLength = 500;

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    private Category(string name, string? description)
    {
        Name = name;
        Description = description;
        IsActive = true;
    }

    /// <summary>Crea una categoría activa.</summary>
    public static Category Create(string? name, string? description)
    {
        return new Category(
            Guard.Required(name, NameMaxLength, "nombre"),
            Guard.Optional(description, DescriptionMaxLength, "descripción"));
    }

    /// <summary>Actualiza el nombre y la descripción.</summary>
    public void Update(string? name, string? description)
    {
        Name = Guard.Required(name, NameMaxLength, "nombre");
        Description = Guard.Optional(description, DescriptionMaxLength, "descripción");
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new DomainException("La categoría ya está inactiva.");
        }

        IsActive = false;
    }

    public void Reactivate()
    {
        if (IsActive)
        {
            throw new DomainException("La categoría ya está activa.");
        }

        IsActive = true;
    }
}
