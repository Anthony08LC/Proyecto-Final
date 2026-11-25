using Microsoft.AspNetCore.Mvc;
using QuoteFlow.Api.Contracts;
using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Documents;
using QuoteFlow.Application.Documents.Queries.GetQuotePdf;
using QuoteFlow.Application.Documents.Queries.GetQuotePreview;
using QuoteFlow.Application.Quotes.Commands.AcceptQuote;
using QuoteFlow.Application.Quotes.Commands.AddQuoteLine;
using QuoteFlow.Application.Quotes.Commands.CancelQuote;
using QuoteFlow.Application.Quotes.Commands.ChangeQuoteCustomer;
using QuoteFlow.Application.Quotes.Commands.ChangeQuoteLineQuantity;
using QuoteFlow.Application.Quotes.Commands.CreateQuote;
using QuoteFlow.Application.Quotes.Commands.DuplicateQuote;
using QuoteFlow.Application.Quotes.Commands.IssueQuote;
using QuoteFlow.Application.Quotes.Commands.RejectQuote;
using QuoteFlow.Application.Quotes.Commands.RemoveQuoteLine;
using QuoteFlow.Application.Quotes.Commands.UpdateQuoteTexts;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Application.Quotes.Queries.GetQuoteById;
using QuoteFlow.Application.Quotes.Queries.GetQuotes;

namespace QuoteFlow.Api.Controllers;

/// <summary>
/// Cotizaciones (sección 4.6). Ventas y Administrador ven todas; editar y cambiar el estado solo lo
/// pueden el propietario o un Administrador (RN-32), lo que verifica cada caso de uso.
/// Las modificaciones de un borrador devuelven el desglose recalculado (estimado).
/// </summary>
public sealed class QuotesController : ApiControllerBase
{
    /// <summary>
    /// Busca cotizaciones por número, cliente, estado, propietario y fechas de creación
    /// (<c>from</c>/<c>to</c> en formato aaaa-mm-dd, hora de Costa Rica). Más recientes primero (RF-COT-14).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<QuoteSummaryDto>>> GetQuotes(
        [FromQuery] GetQuotesQuery query,
        [FromServices] IQueryHandler<GetQuotesQuery, PagedResult<QuoteSummaryDto>> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(query, cancellationToken));

    /// <summary>Detalle con líneas, textos, desglose, historial y acciones permitidas (RF-COT-13).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteDetailDto>> GetQuote(
        Guid id,
        [FromServices] IQueryHandler<GetQuoteByIdQuery, QuoteDetailDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetQuoteByIdQuery(id), cancellationToken));

    /// <summary>
    /// Vista previa HTML del documento comercial, en cualquier estado (RF-DOC-01). Los borradores
    /// llevan la marca de agua «BORRADOR – Sin validez comercial» (RN-36).
    /// </summary>
    [HttpGet("{id:guid}/preview")]
    [Produces("text/html")]
    [ProducesResponseType<string>(StatusCodes.Status200OK, "text/html")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Preview(
        Guid id,
        [FromServices] IQueryHandler<GetQuotePreviewQuery, string> handler,
        CancellationToken cancellationToken)
    {
        var html = await handler.HandleAsync(new GetQuotePreviewQuery(id), cancellationToken);

        // Página generada con datos de usuarios: sin scripts ni recursos externos.
        Response.Headers.ContentSecurityPolicy =
            "default-src 'none'; style-src 'unsafe-inline'; img-src data:; base-uri 'none'; form-action 'none'";
        Response.Headers.XContentTypeOptions = "nosniff";

        return Content(html, "text/html; charset=utf-8");
    }

    /// <summary>
    /// PDF del documento comercial, solo para cotizaciones emitidas (RF-DOC-02, RN-36). Si ya no
    /// está Emitida, el documento muestra su estado. Se descarga como <c>COT-AAAA-000001.pdf</c>.
    /// </summary>
    [HttpGet("{id:guid}/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Pdf(
        Guid id,
        [FromServices] IQueryHandler<GetQuotePdfQuery, QuotePdfFile> handler,
        CancellationToken cancellationToken)
    {
        var file = await handler.HandleAsync(new GetQuotePdfQuery(id), cancellationToken);
        return File(file.Content, "application/pdf", file.FileName);
    }

    /// <summary>Crea un borrador para un cliente activo (RF-COT-01).</summary>
    [HttpPost]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CreatedResponse>> CreateQuote(
        CreateQuoteCommand command,
        [FromServices] ICommandHandler<CreateQuoteCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var id = await handler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetQuote), new { id }, new CreatedResponse(id));
    }

    /// <summary>Agrega un producto al borrador; si ya está, suma la cantidad (RF-COT-02, RN-28).</summary>
    [HttpPost("{id:guid}/lines")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<QuoteBreakdownDto>> AddLine(
        Guid id,
        AddQuoteLineRequest request,
        [FromServices] ICommandHandler<AddQuoteLineCommand, QuoteBreakdownDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(
            new AddQuoteLineCommand(id, request.ProductId, request.Quantity),
            cancellationToken));

    /// <summary>Cambia la cantidad de una línea (RF-COT-03).</summary>
    [HttpPut("{id:guid}/lines/{productId:guid}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<QuoteBreakdownDto>> ChangeLineQuantity(
        Guid id,
        Guid productId,
        ChangeQuantityRequest request,
        [FromServices] ICommandHandler<ChangeQuoteLineQuantityCommand, QuoteBreakdownDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(
            new ChangeQuoteLineQuantityCommand(id, productId, request.Quantity),
            cancellationToken));

    /// <summary>Quita una línea; las restantes se renumeran (RF-COT-03).</summary>
    [HttpDelete("{id:guid}/lines/{productId:guid}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<QuoteBreakdownDto>> RemoveLine(
        Guid id,
        Guid productId,
        [FromServices] ICommandHandler<RemoveQuoteLineCommand, QuoteBreakdownDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new RemoveQuoteLineCommand(id, productId), cancellationToken));

    /// <summary>Cambia el cliente del borrador; el VIP se recalcula (RF-COT-04).</summary>
    [HttpPut("{id:guid}/customer")]
    public async Task<ActionResult<QuoteBreakdownDto>> ChangeCustomer(
        Guid id,
        ChangeQuoteCustomerRequest request,
        [FromServices] ICommandHandler<ChangeQuoteCustomerCommand, QuoteBreakdownDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new ChangeQuoteCustomerCommand(id, request.CustomerId), cancellationToken));

    /// <summary>Edita los cuatro textos comerciales del borrador (RF-COT-05).</summary>
    [HttpPut("{id:guid}/texts")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateTexts(
        Guid id,
        UpdateQuoteTextsRequest request,
        [FromServices] ICommandHandler<UpdateQuoteTextsCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new UpdateQuoteTextsCommand(
                id,
                request.Introduction,
                request.ExecutiveSummary,
                request.Notes,
                request.CommercialTerms),
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Emite el borrador: asigna el número COT-AAAA-000001, la vigencia y congela los datos (RF-COT-07).
    /// Si se rechaza, la respuesta indica la causa exacta.
    /// </summary>
    [HttpPost("{id:guid}/issue")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IssueQuoteResult>> Issue(
        Guid id,
        [FromServices] ICommandHandler<IssueQuoteCommand, IssueQuoteResult> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new IssueQuoteCommand(id), cancellationToken));

    /// <summary>Registra la aceptación dentro de la vigencia (RF-COT-08).</summary>
    [HttpPost("{id:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Accept(
        Guid id,
        [FromServices] ICommandHandler<AcceptQuoteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new AcceptQuoteCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Registra el rechazo con un motivo opcional (RF-COT-09).</summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] ReasonRequest? request,
        [FromServices] ICommandHandler<RejectQuoteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new RejectQuoteCommand(id, request?.Reason), cancellationToken);
        return NoContent();
    }

    /// <summary>Cancela un borrador o una emitida; en una emitida el motivo es obligatorio (RF-COT-10).</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] ReasonRequest? request,
        [FromServices] ICommandHandler<CancelQuoteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new CancelQuoteCommand(id, request?.Reason), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Crea un borrador nuevo a partir de cualquier cotización (RF-COT-12). Informa los productos
    /// inactivos que no se copiaron.
    /// </summary>
    [HttpPost("{id:guid}/duplicate")]
    [ProducesResponseType<DuplicateQuoteResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DuplicateQuoteResult>> Duplicate(
        Guid id,
        [FromServices] ICommandHandler<DuplicateQuoteCommand, DuplicateQuoteResult> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DuplicateQuoteCommand(id), cancellationToken);
        return CreatedAtAction(nameof(GetQuote), new { id = result.QuoteId }, result);
    }
}
