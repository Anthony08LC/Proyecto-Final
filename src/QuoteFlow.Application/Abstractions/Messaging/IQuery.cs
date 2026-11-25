namespace QuoteFlow.Application.Abstractions.Messaging;

/// <summary>Consulta de solo lectura: nunca modifica el estado del sistema.</summary>
public interface IQuery<TResponse>;
