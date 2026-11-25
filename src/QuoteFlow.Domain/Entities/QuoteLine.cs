using QuoteFlow.Domain.Common;
using QuoteFlow.Domain.Exceptions;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Domain.Entities;

/// <summary>
/// Línea de una cotización. Forma parte del agregado <see cref="Quote"/>:
/// solo la cotización puede crearla o modificarla.
/// Guarda una copia de los datos del producto y su categoría, que se congela al emitir (RN-12).
/// </summary>
public sealed class QuoteLine : Entity
{
    public const int QuantityDecimals = 2;

    public Guid ProductId { get; private set; }

    public int LineNumber { get; private set; }

    public string ProductName { get; private set; }

    public string? ProductDescription { get; private set; }

    public string CategoryName { get; private set; }

    public decimal Quantity { get; private set; }

    public Money UnitPrice { get; private set; }

    public Percentage DiscountPercentage { get; private set; }

    private QuoteLine(
        Guid productId,
        int lineNumber,
        string productName,
        string? productDescription,
        string categoryName,
        decimal quantity,
        Money unitPrice,
        Percentage discountPercentage)
    {
        ProductId = productId;
        LineNumber = lineNumber;
        ProductName = productName;
        ProductDescription = productDescription;
        CategoryName = categoryName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercentage = discountPercentage;
    }

    /// <summary>Crea una línea con el precio y los datos actuales del catálogo.</summary>
    internal static QuoteLine Create(
        Product product,
        Category category,
        decimal quantity,
        int lineNumber,
        Percentage? discountPercentage)
    {
        EnsureCatalogData(product, category);
        EnsureValidQuantity(quantity);
        EnsureValidLineNumber(lineNumber);

        return new QuoteLine(
            product.Id,
            lineNumber,
            product.Name,
            product.Description,
            category.Name,
            quantity,
            product.Price,
            discountPercentage ?? Percentage.Zero);
    }

    /// <summary>
    /// Total de la línea: redondear(cantidad × precio unitario) (RN-17),
    /// menos el descuento de la línea, también redondeado.
    /// </summary>
    public Money CalculateSubtotal()
    {
        var gross = UnitPrice.Multiply(Quantity);
        return gross - gross.PercentageOf(DiscountPercentage);
    }

    internal void ChangeQuantity(decimal quantity)
    {
        EnsureValidQuantity(quantity);
        Quantity = quantity;
    }

    /// <summary>Suma cantidad a la línea cuando el producto se agrega de nuevo (RN-28).</summary>
    internal void IncreaseQuantity(decimal quantity)
    {
        EnsureValidQuantity(quantity);
        Quantity += quantity;
    }

    internal void ChangeDiscountPercentage(Percentage discountPercentage)
    {
        DiscountPercentage = discountPercentage
            ?? throw new DomainException("El porcentaje de descuento de la línea es obligatorio.");
    }

    internal void ChangeLineNumber(int lineNumber)
    {
        EnsureValidLineNumber(lineNumber);
        LineNumber = lineNumber;
    }

    /// <summary>Actualiza el precio y los datos copiados con los valores vigentes del catálogo (RN-22).</summary>
    internal void RefreshFromCatalog(Product product, Category category)
    {
        EnsureCatalogData(product, category);

        if (product.Id != ProductId)
        {
            throw new DomainException("El producto no corresponde a la línea de la cotización.");
        }

        ProductName = product.Name;
        ProductDescription = product.Description;
        CategoryName = category.Name;
        UnitPrice = product.Price;
    }

    private static void EnsureCatalogData(Product? product, Category? category)
    {
        if (product is null)
        {
            throw new DomainException("El producto de la línea es obligatorio.");
        }

        if (category is null)
        {
            throw new DomainException("La categoría del producto es obligatoria.");
        }

        if (product.CategoryId != category.Id)
        {
            throw new DomainException("La categoría indicada no corresponde al producto.");
        }
    }

    private static void EnsureValidQuantity(decimal quantity)
    {
        if (quantity <= 0m)
        {
            throw new DomainException("La cantidad debe ser mayor que cero.");
        }

        if (decimal.Round(quantity, QuantityDecimals) != quantity)
        {
            throw new DomainException(
                $"La cantidad no puede tener más de {QuantityDecimals} decimales.");
        }
    }

    private static void EnsureValidLineNumber(int lineNumber)
    {
        if (lineNumber <= 0)
        {
            throw new DomainException("El número de línea debe ser mayor que cero.");
        }
    }
}
