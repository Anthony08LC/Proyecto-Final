using Microsoft.AspNetCore.Mvc;
using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Assistant.Queries.GetAssistantStatus;

namespace QuoteFlow.Api.Controllers;

/// <summary>
/// Estado del asistente de redacción con IA (sección 11). Las sugerencias se piden por cotización:
/// <c>POST /api/quotes/{id}/assistant</c>.
/// </summary>
public sealed class AssistantController : ApiControllerBase
{
    /// <summary>Indica si el asistente está disponible y cuántas solicitudes le quedan al usuario esta hora.</summary>
    [HttpGet("status")]
    public async Task<ActionResult<AssistantStatusDto>> GetStatus(
        [FromServices] IQueryHandler<GetAssistantStatusQuery, AssistantStatusDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetAssistantStatusQuery(), cancellationToken));
}
