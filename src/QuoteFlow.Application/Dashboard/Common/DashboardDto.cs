using QuoteFlow.Application.Common.Models;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Dashboard.Common;

/// <summary>Página de inicio: indicadores, cotizaciones recientes y actividad reciente (RF-DSH-01 a 03).</summary>
public sealed record DashboardDto(
    DashboardIndicatorsDto Indicators,
    IReadOnlyList<RecentQuoteDto> RecentQuotes,
    IReadOnlyList<RecentActivityDto> RecentActivity);

/// <summary>
/// Indicadores globales (RF-DSH-01). <c>NumberedQuotes</c>: todas menos los borradores.
/// <c>QuotedAmount</c>: suma de Emitidas, Aceptadas, Rechazadas y Vencidas (sin borradores ni canceladas).
/// </summary>
public sealed record DashboardIndicatorsDto(
    int ActiveCustomers,
    int ActiveProducts,
    int NumberedQuotes,
    decimal QuotedAmount);

/// <summary>Cotización reciente (RF-DSH-02). <c>Number</c> es nulo en los borradores.</summary>
public sealed record RecentQuoteDto(
    Guid Id,
    string? Number,
    string CustomerName,
    QuoteStatus Status,
    decimal Total,
    bool IsEstimate,
    DateTimeOffset CreatedAt);

/// <summary>Evento reciente (RF-DSH-03). La fecha está en UTC.</summary>
public sealed record RecentActivityDto(
    long Id,
    DateTimeOffset OccurredAt,
    string? UserName,
    string EventType,
    AuditEntityType EntityType,
    Guid? EntityId,
    string Detail);
