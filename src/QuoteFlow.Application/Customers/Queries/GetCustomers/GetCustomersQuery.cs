using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Customers.Common;

namespace QuoteFlow.Application.Customers.Queries.GetCustomers;

/// <summary>
/// Busca clientes por nombre, empresa o correo, con filtros por estado y condición VIP.
/// Resultado paginado y ordenado por nombre (RF-CLI-02).
/// </summary>
public sealed record GetCustomersQuery(
    string? Search = null,
    bool? IsActive = null,
    bool? IsVip = null,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize) : IQuery<PagedResult<CustomerSummaryDto>>;
