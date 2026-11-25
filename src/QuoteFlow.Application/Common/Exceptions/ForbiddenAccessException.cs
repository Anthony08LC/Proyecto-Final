namespace QuoteFlow.Application.Common.Exceptions;

/// <summary>
/// El usuario no tiene permiso para la operación (por ejemplo, RN-21 o RN-32). Se traduce a HTTP 403.
/// </summary>
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "No tiene permiso para realizar esta acción.")
        : base(message)
    {
    }
}
