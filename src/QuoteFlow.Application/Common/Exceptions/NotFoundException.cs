namespace QuoteFlow.Application.Common.Exceptions;

/// <summary>El recurso solicitado no existe. Se traduce a HTTP 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"No se encontró {entityName} con identificador «{key}».")
    {
    }
}
