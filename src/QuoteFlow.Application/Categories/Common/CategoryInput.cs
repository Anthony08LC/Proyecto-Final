using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Categories.Common;

/// <summary>Datos de entrada de una categoría ya validados.</summary>
internal sealed record CategoryInput(string Name, string? Description)
{
    public static CategoryInput Parse(string? name, string? description)
    {
        var errors = new ValidationErrors();
        var parsedName = errors.Required("name", "El nombre", name, Category.NameMaxLength);
        var parsedDescription = errors.Optional(
            "description",
            "La descripción",
            description,
            Category.DescriptionMaxLength);

        errors.ThrowIfAny();

        return new CategoryInput(parsedName!, parsedDescription);
    }
}
