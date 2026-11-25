namespace QuoteFlow.Application.Customers.Common;

/// <summary>Detalle completo de un cliente.</summary>
public sealed record CustomerDto(
    Guid Id,
    string Name,
    string? Company,
    string? Email,
    string? Phone,
    bool IsVip,
    bool IsActive,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
