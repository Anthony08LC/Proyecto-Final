using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoteFlow.Api.Contracts;
using QuoteFlow.Api.Security;
using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Products.Commands.CreateProduct;
using QuoteFlow.Application.Products.Commands.DeactivateProduct;
using QuoteFlow.Application.Products.Commands.ReactivateProduct;
using QuoteFlow.Application.Products.Commands.UpdateProduct;
using QuoteFlow.Application.Products.Common;
using QuoteFlow.Application.Products.Queries.GetProductById;
using QuoteFlow.Application.Products.Queries.GetProducts;

namespace QuoteFlow.Api.Controllers;

/// <summary>Catálogo de productos (sección 4.5). Consulta: todos. Gestión: solo Administrador.</summary>
public sealed class ProductsController : ApiControllerBase
{
    /// <summary>Busca productos por nombre, con filtros por categoría y estado (RF-PRO-02).</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] GetProductsQuery query,
        [FromServices] IQueryHandler<GetProductsQuery, PagedResult<ProductDto>> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(query, cancellationToken));

    /// <summary>Detalle de un producto.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(
        Guid id,
        [FromServices] IQueryHandler<GetProductByIdQuery, ProductDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetProductByIdQuery(id), cancellationToken));

    /// <summary>Crea un producto (RF-PRO-01).</summary>
    [HttpPost]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedResponse>> CreateProduct(
        CreateProductCommand command,
        [FromServices] ICommandHandler<CreateProductCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var id = await handler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id }, new CreatedResponse(id));
    }

    /// <summary>Edita un producto, incluido su precio (RF-PRO-03).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        UpdateProductRequest request,
        [FromServices] ICommandHandler<UpdateProductCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new UpdateProductCommand(id, request.Name, request.Description, request.CategoryId, request.Price),
            cancellationToken);
        return NoContent();
    }

    /// <summary>Desactiva un producto (RF-PRO-04).</summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivateProduct(
        Guid id,
        [FromServices] ICommandHandler<DeactivateProductCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeactivateProductCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Reactiva un producto (RF-PRO-04).</summary>
    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReactivateProduct(
        Guid id,
        [FromServices] ICommandHandler<ReactivateProductCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ReactivateProductCommand(id), cancellationToken);
        return NoContent();
    }
}
