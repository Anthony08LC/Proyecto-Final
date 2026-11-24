using System.Text.RegularExpressions;
using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Domain.ValueObjects;

/// <summary>
/// Correo electrónico válido, normalizado a minúsculas y sin espacios. Inmutable.
/// </summary>
public sealed partial record Email
{
    public const int MaxLength = 254;

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>Crea un correo obligatorio; lanza <see cref="DomainException"/> si no es válido.</summary>
    public static Email Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("El correo electrónico es obligatorio.");
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
        {
            throw new DomainException(
                $"El correo electrónico no puede superar los {MaxLength} caracteres.");
        }

        if (!EmailPattern().IsMatch(normalized))
        {
            throw new DomainException("El correo electrónico no tiene un formato válido.");
        }

        return new Email(normalized);
    }

    /// <summary>
    /// Crea un correo opcional: devuelve <c>null</c> si el valor está vacío
    /// y valida el formato si se indica.
    /// </summary>
    public static Email? CreateOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : Create(value);

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailPattern();
}
