using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoteFlow.Api.Contracts;
using QuoteFlow.Api.Security;
using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Categories.Commands.CreateCategory;
using QuoteFlow.Application.Categories.Commands.DeactivateCategory;
using QuoteFlow.Application.Categories.Commands.ReactivateCategory;
using QuoteFlow.Application.Categories.Commands.UpdateCategory;
using QuoteFlow.Application.Categories.Common;
using QuoteFlow.Application.Categories.Queries.GetCategories;
using QuoteFlow.Application.Categories.Queries.GetCategoryById;
using QuoteFlow.Application.Common.Models;

namespace QuoteFlow.Api.Controllers;

/// <summary>
/// Categorías de productos (sección 4.4). Consulta: todos los usuarios (filtro del catálogo).
/// Gestión: solo Administrador.
/// </summary>
public sealed class CategoriesController : ApiControllerBase
{
    /// <summary>Lista las categorías con su estado y cantidad de productos (RF-CAT-02).</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<CategoryDto>>> GetCategories(
        [FromQuery] GetCategoriesQuery query,
        [FromServices] IQueryHandler<GetCategoriesQuery, PagedResult<CategoryDto>> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(query, cancellationToken));

    /// <summary>Detalle de una categoría.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetCategory(
        Guid id,
        [FromServices] IQueryHandler<GetCategoryByIdQuery, CategoryDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetCategoryByIdQuery(id), cancellationToken));

    /// <summary>Crea una categoría (RF-CAT-01). Si el nombre coincide con una inactiva, sugiere reactivarla.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedResponse>> CreateCategory(
        CreateCategoryCommand command,
        [FromServices] ICommandHandler<CreateCategoryCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var id = await handler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetCategory), new { id }, new CreatedResponse(id));
    }

    /// <summary>Edita una categoría (RF-CAT-03).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        UpdateCategoryRequest request,
        [FromServices] ICommandHandler<UpdateCategoryCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UpdateCategoryCommand(id, request.Name, request.Description), cancellationToken);
        return NoContent();
    }

    /// <summary>Desactiva una categoría (RF-CAT-04).</summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivateCategory(
        Guid id,
        [FromServices] ICommandHandler<DeactivateCategoryCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeactivateCategoryCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Reactiva una categoría (RF-CAT-04).</summary>
    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReactivateCategory(
        Guid id,
        [FromServices] ICommandHandler<ReactivateCategoryCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ReactivateCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}
