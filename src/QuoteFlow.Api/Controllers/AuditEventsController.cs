using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoteFlow.Api.Security;
using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Auditing.Common;
using QuoteFlow.Application.Auditing.Queries.GetAuditEvents;
using QuoteFlow.Application.Common.Models;

namespace QuoteFlow.Api.Controllers;

/// <summary>Consulta de auditoría (sección 4.10). Solo lectura y solo Administrador.</summary>
[Authorize(Policy = Policies.Administrator)]
[Route("api/audit-events")]
public sealed class AuditEventsController : ApiControllerBase
{
    /// <summary>
    /// Eventos filtrados por usuario, tipo de evento, tipo de entidad y rango de fechas
    /// (en hora de Costa Rica); más recientes primero y paginado (RF-AUD-02).
    /// </summary>
    [HttpGet]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<AuditEventDto>>> GetAuditEvents(
        [FromQuery] GetAuditEventsQuery query,
        [FromServices] IQueryHandler<GetAuditEventsQuery, PagedResult<AuditEventDto>> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(query, cancellationToken));
}
