namespace QuoteFlow.Application.Common.Exceptions;

/// <summary>
/// La operación entra en conflicto con el estado actual: un nombre o correo repetido (RN-30)
/// o una modificación concurrente (RNF-DAT-04). Se traduce a HTTP 409.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
