using QuoteFlow.Domain.Common;
using QuoteFlow.Domain.Exceptions;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Domain.Entities;

/// <summary>
/// Producto del catálogo. Pertenece a una categoría, que debe estar activa al crear
/// o editar el producto (RN-29). No se elimina físicamente: se desactiva (RN-20).
/// La unicidad del nombre (RN-30) se valida en la capa de aplicación.
/// </summary>
public sealed class Product : AuditableEntity
{
    public const int NameMaxLength = 150;
    public const int DescriptionMaxLength = 1000;

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public Guid CategoryId { get; private set; }

    public Money Price { get; private set; }

    public bool IsActive { get; private set; }

    private Product(string name, string? description, Guid categoryId, Money price)
    {
        Name = name;
        Description = description;
        CategoryId = categoryId;
        Price = price;
        IsActive = true;
    }

    /// <summary>Crea un producto activo en una categoría activa.</summary>
    public static Product Create(
        string? name,
        string? description,
        Category category,
        Money price)
    {
        EnsureActiveCategory(category);
        EnsurePrice(price);

        return new Product(
            Guard.Required(name, NameMaxLength, "nombre"),
            Guard.Optional(description, DescriptionMaxLength, "descripción"),
            category.Id,
            price);
    }

    /// <summary>Actualiza los datos del producto; la categoría debe estar activa (RN-29).</summary>
    public void Update(
        string? name,
        string? description,
        Category category,
        Money price)
    {
        EnsureActiveCategory(category);
        EnsurePrice(price);

        Name = Guard.Required(name, NameMaxLength, "nombre");
        Description = Guard.Optional(description, DescriptionMaxLength, "descripción");
        CategoryId = category.Id;
        Price = price;
    }

    public void ChangePrice(Money price)
    {
        EnsurePrice(price);
        Price = price;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            throw new DomainException("El producto ya está inactivo.");
        }

        IsActive = false;
    }

    public void Reactivate()
    {
        if (IsActive)
        {
            throw new DomainException("El producto ya está activo.");
        }

        IsActive = true;
    }

    private static void EnsureActiveCategory(Category? category)
    {
        if (category is null)
        {
            throw new DomainException("La categoría es obligatoria.");
        }

        if (!category.IsActive)
        {
            throw new DomainException("La categoría seleccionada está inactiva.");
        }
    }

    private static void EnsurePrice(Money? price)
    {
        if (price is null)
        {
            throw new DomainException("El precio es obligatorio.");
        }
    }
}
