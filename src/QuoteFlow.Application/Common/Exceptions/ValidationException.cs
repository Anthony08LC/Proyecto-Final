namespace QuoteFlow.Application.Common.Exceptions;

/// <summary>
/// Los datos de entrada no son válidos. Agrupa los errores por campo. Se traduce a HTTP 400.
/// </summary>
public sealed class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Uno o más datos no son válidos.")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { [field] = [error] })
    {
    }
}
