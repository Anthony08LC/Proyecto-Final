namespace QuoteFlow.Application.Abstractions.Messaging;

/// <summary>Manejador de una consulta.</summary>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
