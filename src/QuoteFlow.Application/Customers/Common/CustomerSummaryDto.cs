namespace QuoteFlow.Application.Customers.Common;

/// <summary>Datos de un cliente para listados y búsquedas.</summary>
public sealed record CustomerSummaryDto(
    Guid Id,
    string Name,
    string? Company,
    string? Email,
    string? Phone,
    bool IsVip,
    bool IsActive);
