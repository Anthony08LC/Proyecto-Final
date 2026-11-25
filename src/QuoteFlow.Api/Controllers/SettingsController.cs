using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoteFlow.Api.Security;
using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Settings.Commands.UpdateCommercialParameters;
using QuoteFlow.Application.Settings.Commands.UpdateCompanySettings;
using QuoteFlow.Application.Settings.Common;
using QuoteFlow.Application.Settings.Queries.GetCommercialParametersImpact;
using QuoteFlow.Application.Settings.Queries.GetSystemSettings;

namespace QuoteFlow.Api.Controllers;

/// <summary>Configuración del sistema (sección 4.9). Solo Administrador.</summary>
[Authorize(Policy = Policies.Administrator)]
public sealed class SettingsController : ApiControllerBase
{
    /// <summary>Consulta la configuración (RF-CFG-01).</summary>
    [HttpGet]
    public async Task<ActionResult<SystemSettingsDto>> GetSettings(
        [FromServices] IQueryHandler<GetSystemSettingsQuery, SystemSettingsDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetSystemSettingsQuery(), cancellationToken));

    /// <summary>Actualiza los datos de la empresa y las condiciones por defecto (RF-CFG-02).</summary>
    [HttpPut("company")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateCompany(
        UpdateCompanySettingsCommand command,
        [FromServices] ICommandHandler<UpdateCompanySettingsCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    /// <summary>Informa cuántos borradores se recalcularán antes de cambiar los parámetros (RF-CFG-03).</summary>
    [HttpGet("commercial-parameters/impact")]
    public async Task<ActionResult<CommercialParametersImpactDto>> GetCommercialParametersImpact(
        [FromServices] IQueryHandler<GetCommercialParametersImpactQuery, CommercialParametersImpactDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetCommercialParametersImpactQuery(), cancellationToken));

    /// <summary>Actualiza IVA, tramos, descuento VIP y vigencia (RF-CFG-03). Solo afecta a los borradores.</summary>
    [HttpPut("commercial-parameters")]
    public async Task<ActionResult<CommercialParametersImpactDto>> UpdateCommercialParameters(
        UpdateCommercialParametersCommand command,
        [FromServices] ICommandHandler<UpdateCommercialParametersCommand, CommercialParametersImpactDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(command, cancellationToken));
}
