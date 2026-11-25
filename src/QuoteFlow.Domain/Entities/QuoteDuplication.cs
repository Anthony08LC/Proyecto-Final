namespace QuoteFlow.Domain.Entities;

/// <summary>
/// Resultado de duplicar una cotización (RF-COT-12): el borrador nuevo y los nombres de los
/// productos que no se copiaron por estar inactivos (RN-25).
/// </summary>
public sealed record QuoteDuplication(Quote Copy, IReadOnlyList<string> OmittedProducts);
