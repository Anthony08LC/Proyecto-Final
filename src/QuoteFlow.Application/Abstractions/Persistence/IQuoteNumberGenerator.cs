namespace QuoteFlow.Application.Abstractions.Persistence;

/// <summary>
/// Genera el consecutivo de cotización <c>COT-AAAA-000001</c> (RN-13): único, sin saltos y
/// reiniciado cada año. Debe ejecutarse dentro de la transacción de emisión, bloqueando
/// la fila del año, para que una emisión fallida no consuma número.
/// </summary>
public interface IQuoteNumberGenerator
{
    Task<string> NextAsync(int year, CancellationToken cancellationToken = default);
}
