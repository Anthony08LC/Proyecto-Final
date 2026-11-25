namespace QuoteFlow.Application.Common.Exceptions;

/// <summary>
/// Las credenciales no son válidas o el usuario no puede iniciar sesión. El mensaje es siempre
/// genérico para no revelar si el correo existe (RF-AUT-01). Se traduce a HTTP 401.
/// </summary>
public sealed class AuthenticationFailedException : Exception
{
    public const string GenericMessage = "Correo o contraseña incorrectos.";

    public AuthenticationFailedException(string message = GenericMessage)
        : base(message)
    {
    }
}
