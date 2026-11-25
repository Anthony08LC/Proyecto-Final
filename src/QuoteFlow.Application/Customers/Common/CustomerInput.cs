using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Exceptions;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Application.Customers.Common;

/// <summary>
/// Datos de un cliente ya validados. <see cref="Parse"/> revisa todos los campos y, si alguno
/// no es válido, lanza una <see cref="ValidationException"/> con todos los errores por campo.
/// </summary>
internal sealed record CustomerInput(
    string Name,
    string? Company,
    Email? Email,
    Phone? Phone,
    string? Notes)
{
    public static CustomerInput Parse(
        string? name,
        string? company,
        string? email,
        string? phone,
        string? notes)
    {
        var errors = new Dictionary<string, string[]>();

        var trimmedName = name?.Trim();
        if (string.IsNullOrEmpty(trimmedName))
        {
            errors["name"] = ["El nombre es obligatorio."];
        }
        else if (trimmedName.Length > Customer.NameMaxLength)
        {
            errors["name"] = [$"El nombre no puede superar los {Customer.NameMaxLength} caracteres."];
        }

        CheckMaxLength(errors, "company", "La empresa", company, Customer.CompanyMaxLength);
        CheckMaxLength(errors, "notes", "Las observaciones", notes, Customer.NotesMaxLength);

        var parsedEmail = TryParse(errors, "email", () => Email.CreateOptional(email));
        var parsedPhone = TryParse(errors, "phone", () => Phone.CreateOptional(phone));

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        return new CustomerInput(trimmedName!, company, parsedEmail, parsedPhone, notes);
    }

    private static void CheckMaxLength(
        Dictionary<string, string[]> errors,
        string field,
        string label,
        string? value,
        int maxLength)
    {
        if (value is not null && value.Trim().Length > maxLength)
        {
            errors[field] = [$"{label} no puede superar los {maxLength} caracteres."];
        }
    }

    private static T? TryParse<T>(Dictionary<string, string[]> errors, string field, Func<T?> parse)
        where T : class
    {
        try
        {
            return parse();
        }
        catch (DomainException ex)
        {
            errors[field] = [ex.Message];
            return null;
        }
    }
}
