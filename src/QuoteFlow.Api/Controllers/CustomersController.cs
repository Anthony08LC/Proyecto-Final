using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoteFlow.Api.Contracts;
using QuoteFlow.Api.Security;
using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Customers.Commands.CreateCustomer;
using QuoteFlow.Application.Customers.Commands.DeactivateCustomer;
using QuoteFlow.Application.Customers.Commands.MarkCustomerAsVip;
using QuoteFlow.Application.Customers.Commands.ReactivateCustomer;
using QuoteFlow.Application.Customers.Commands.RemoveCustomerVip;
using QuoteFlow.Application.Customers.Commands.UpdateCustomer;
using QuoteFlow.Application.Customers.Common;
using QuoteFlow.Application.Customers.Queries.GetCustomerById;
using QuoteFlow.Application.Customers.Queries.GetCustomers;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Application.Quotes.Queries.GetQuotes;

namespace QuoteFlow.Api.Controllers;

/// <summary>
/// Cartera de clientes (sección 4.3). Registrar y consultar: Ventas y Administrador (Ventas sin VIP,
/// RN-21). Editar, cambiar VIP, desactivar y reactivar: solo Administrador.
/// </summary>
public sealed class CustomersController : ApiControllerBase
{
    /// <summary>Busca clientes por nombre, empresa o correo, con filtros por estado y VIP (RF-CLI-02).</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerSummaryDto>>> GetCustomers(
        [FromQuery] GetCustomersQuery query,
        [FromServices] IQueryHandler<GetCustomersQuery, PagedResult<CustomerSummaryDto>> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(query, cancellationToken));

    /// <summary>Detalle de un cliente (RF-CLI-03).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetCustomer(
        Guid id,
        [FromServices] IQueryHandler<GetCustomerByIdQuery, CustomerDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetCustomerByIdQuery(id), cancellationToken));

    /// <summary>Cotizaciones del cliente, más recientes primero (RF-CLI-03).</summary>
    [HttpGet("{id:guid}/quotes")]
    public async Task<ActionResult<PagedResult<QuoteSummaryDto>>> GetCustomerQuotes(
        Guid id,
        [FromServices] IQueryHandler<GetQuotesQuery, PagedResult<QuoteSummaryDto>> handler,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PageRequest.DefaultPageSize) =>
        Ok(await handler.HandleAsync(
            new GetQuotesQuery(CustomerId: id, Page: page, PageSize: pageSize),
            cancellationToken));

    /// <summary>
    /// Registra un cliente (RF-CLI-01). <c>possibleDuplicateEmail</c> advierte si otro cliente ya usa
    /// el correo; el registro se completa igual.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<CustomerSaveResult>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CustomerSaveResult>> CreateCustomer(
        CreateCustomerCommand command,
        [FromServices] ICommandHandler<CreateCustomerCommand, CustomerSaveResult> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetCustomer), new { id = result.CustomerId }, result);
    }

    /// <summary>Edita un cliente (RF-CLI-04). No afecta a las cotizaciones emitidas.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Administrator)]
    public async Task<ActionResult<CustomerSaveResult>> UpdateCustomer(
        Guid id,
        UpdateCustomerRequest request,
        [FromServices] ICommandHandler<UpdateCustomerCommand, CustomerSaveResult> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(
            new UpdateCustomerCommand(id, request.Name, request.Company, request.Email, request.Phone, request.Notes),
            cancellationToken));

    /// <summary>Marca al cliente como VIP (RF-CLI-05).</summary>
    [HttpPut("{id:guid}/vip")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAsVip(
        Guid id,
        [FromServices] ICommandHandler<MarkCustomerAsVipCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new MarkCustomerAsVipCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Quita la condición VIP del cliente (RF-CLI-05).</summary>
    [HttpDelete("{id:guid}/vip")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveVip(
        Guid id,
        [FromServices] ICommandHandler<RemoveCustomerVipCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new RemoveCustomerVipCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Desactiva un cliente (RF-CLI-06).</summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivateCustomer(
        Guid id,
        [FromServices] ICommandHandler<DeactivateCustomerCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeactivateCustomerCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Reactiva un cliente (RF-CLI-06).</summary>
    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReactivateCustomer(
        Guid id,
        [FromServices] ICommandHandler<ReactivateCustomerCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ReactivateCustomerCommand(id), cancellationToken);
        return NoContent();
    }
}
