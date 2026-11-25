using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Quotes.Queries.GetQuotes;

/// <summary>
/// Consulta y busca cotizaciones por número, cliente, estado, propietario y rango de fechas de
/// creación (en hora de Costa Rica, ambos extremos inclusivos). Más recientes primero y paginado
/// (RF-COT-14). Ventas ve todas las cotizaciones (RN-32). Con <c>CustomerId</c> sirve también para
/// mostrar las cotizaciones en el detalle de un cliente (RF-CLI-03).
/// </summary>
public sealed record GetQuotesQuery(
    string? Number = null,
    Guid? CustomerId = null,
    QuoteStatus? Status = null,
    Guid? OwnerId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize) : IQuery<PagedResult<QuoteSummaryDto>>;
