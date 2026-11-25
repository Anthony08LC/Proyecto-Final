using Microsoft.AspNetCore.Mvc;
using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Dashboard.Common;
using QuoteFlow.Application.Dashboard.Queries.GetDashboard;

namespace QuoteFlow.Api.Controllers;

/// <summary>Dashboard (sección 4.11). Administrador y Ventas.</summary>
public sealed class DashboardController : ApiControllerBase
{
    /// <summary>
    /// Indicadores globales, las 5 cotizaciones más recientes y los últimos 10 eventos
    /// (de todos los usuarios para el Administrador; solo los propios para Ventas) (RF-DSH-01 a 03).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard(
        [FromServices] IQueryHandler<GetDashboardQuery, DashboardDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetDashboardQuery(), cancellationToken));
}
