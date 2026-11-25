using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Application.Common.Validation;

/// <summary>
/// Acumula los errores de validación de entrada por campo, para informar todos a la vez
/// (RNF-SEG-06, RNF-USA-04). <see cref="ThrowIfAny"/> lanza una <see cref="ValidationException"/>.
/// </summary>
internal sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

    public bool HasErrors => _errors.Count > 0;

    public void Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var messages))
        {
            messages = [];
            _errors[field] = messages;
        }

        messages.Add(message);
    }

    /// <summary>Valida un texto obligatorio y devuelve su valor sin espacios al inicio ni al final.</summary>
    public string? Required(string field, string label, string? value, int maxLength)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            Add(field, $"{label} es obligatorio.");
            return null;
        }

        return MaxLength(field, label, trimmed, maxLength);
    }

    /// <summary>Valida un texto opcional; devuelve <c>null</c> si está vacío.</summary>
    public string? Optional(string field, string label, string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : MaxLength(field, label, trimmed, maxLength);
    }

    /// <summary>
    /// Ejecuta una conversión del dominio (por ejemplo, crear un value object) y registra
    /// como error del campo cualquier <see cref="DomainException"/>.
    /// </summary>
    public T? Capture<T>(string field, Func<T?> parse)
        where T : class
    {
        try
        {
            return parse();
        }
        catch (DomainException ex)
        {
            Add(field, ex.Message);
            return null;
        }
    }

    public void ThrowIfAny()
    {
        if (HasErrors)
        {
            throw new ValidationException(_errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));
        }
    }

    private string? MaxLength(string field, string label, string value, int maxLength)
    {
        if (value.Length > maxLength)
        {
            Add(field, $"{label} no puede superar los {maxLength} caracteres.");
            return null;
        }

        return value;
    }
}
