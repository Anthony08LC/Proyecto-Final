using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Application.Products.Common;

/// <summary>Datos de entrada de un producto ya validados (RF-PRO-01).</summary>
internal sealed record ProductInput(string Name, string? Description, Guid CategoryId, Money Price)
{
    public static ProductInput Parse(string? name, string? description, Guid categoryId, decimal price)
    {
        var errors = new ValidationErrors();

        var parsedName = errors.Required("name", "El nombre", name, Product.NameMaxLength);
        var parsedDescription = errors.Optional(
            "description",
            "La descripción",
            description,
            Product.DescriptionMaxLength);

        if (categoryId == Guid.Empty)
        {
            errors.Add("categoryId", "La categoría es obligatoria.");
        }

        var parsedPrice = errors.Capture("price", () => Money.Create(price));

        errors.ThrowIfAny();

        return new ProductInput(parsedName!, parsedDescription, categoryId, parsedPrice!);
    }
}
