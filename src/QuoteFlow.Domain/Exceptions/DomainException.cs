namespace QuoteFlow.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se viola una invariante o una regla de negocio del dominio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
