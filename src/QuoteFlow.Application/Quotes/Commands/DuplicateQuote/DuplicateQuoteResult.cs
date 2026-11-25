namespace QuoteFlow.Application.Quotes.Commands.DuplicateQuote;

/// <summary>Borrador creado y nombres de los productos que no se copiaron por estar inactivos.</summary>
public sealed record DuplicateQuoteResult(Guid QuoteId, IReadOnlyList<string> OmittedProducts);
