using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Domain.Common;

/// <summary>
/// Validaciones de texto reutilizables por las entidades del dominio.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Valida un texto obligatorio y devuelve su valor sin espacios al inicio ni al final.
    /// </summary>
    public static string Required(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"El campo {fieldName} es obligatorio.");
        }

        return EnsureMaxLength(value.Trim(), maxLength, fieldName);
    }

    /// <summary>
    /// Valida un texto opcional: devuelve <c>null</c> si está vacío o su valor sin espacios
    /// al inicio ni al final.
    /// </summary>
    public static string? Optional(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return EnsureMaxLength(value.Trim(), maxLength, fieldName);
    }

    private static string EnsureMaxLength(string value, int maxLength, string fieldName)
    {
        if (value.Length > maxLength)
        {
            throw new DomainException(
                $"El campo {fieldName} no puede superar los {maxLength} caracteres.");
        }

        return value;
    }
}
