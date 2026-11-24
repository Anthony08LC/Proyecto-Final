using System.Text.RegularExpressions;
using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Domain.ValueObjects;

/// <summary>
/// Teléfono de Costa Rica: 8 dígitos con prefijo <c>+506</c> opcional.
/// Se normaliza siempre al formato <c>+506XXXXXXXX</c>. Inmutable.
/// </summary>
public sealed partial record Phone
{
    public const string CountryPrefix = "+506";
    public const int MaxLength = 13;

    public string Value { get; }

    private Phone(string value)
    {
        Value = value;
    }

    /// <summary>Crea un teléfono obligatorio; lanza <see cref="DomainException"/> si no es válido.</summary>
    public static Phone Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("El teléfono es obligatorio.");
        }

        var compact = SeparatorPattern().Replace(value.Trim(), string.Empty);
        var match = PhonePattern().Match(compact);

        if (!match.Success)
        {
            throw new DomainException(
                "El teléfono debe tener 8 dígitos, con el prefijo +506 opcional.");
        }

        return new Phone(CountryPrefix + match.Groups["number"].Value);
    }

    /// <summary>
    /// Crea un teléfono opcional: devuelve <c>null</c> si el valor está vacío
    /// y valida el formato si se indica.
    /// </summary>
    public static Phone? CreateOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : Create(value);

    public override string ToString() => Value;

    [GeneratedRegex(@"[\s\-().]")]
    private static partial Regex SeparatorPattern();

    [GeneratedRegex(@"^(?:\+506)?(?<number>\d{8})$", RegexOptions.CultureInvariant)]
    private static partial Regex PhonePattern();
}
