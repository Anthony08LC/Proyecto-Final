namespace QuoteFlow.Application.Quotes.Common;

/// <summary>
/// Cliente de la cotización: datos vigentes en borradores y datos congelados al emitir (RN-12).
/// <paramref name="IsActive"/> es siempre el estado actual del cliente.
/// </summary>
public sealed record QuoteCustomerDto(
    Guid Id,
    string Name,
    string? Company,
    string? Email,
    string? Phone,
    bool IsVip,
    bool IsActive);
