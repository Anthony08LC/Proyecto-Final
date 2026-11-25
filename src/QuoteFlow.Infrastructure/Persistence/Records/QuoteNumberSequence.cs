namespace QuoteFlow.Infrastructure.Persistence.Records;

/// <summary>
/// Fila de <c>quote_number_sequences</c>: último consecutivo emitido en un año (RN-13).
/// Solo la modifica <see cref="QuoteNumberGenerator"/>, bloqueando la fila dentro de la
/// transacción de emisión.
/// </summary>
internal sealed class QuoteNumberSequence
{
    public const int MinYear = 2000;
    public const int MaxYear = 9999;
    public const int MaxNumber = 999_999;

    public short Year { get; init; }

    public int LastNumber { get; init; }
}
