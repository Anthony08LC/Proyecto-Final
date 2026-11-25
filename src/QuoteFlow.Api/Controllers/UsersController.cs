using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuoteFlow.Api.Contracts;
using QuoteFlow.Api.Security;
using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Users.Commands.CreateUser;
using QuoteFlow.Application.Users.Commands.DeactivateUser;
using QuoteFlow.Application.Users.Commands.ReactivateUser;
using QuoteFlow.Application.Users.Commands.ResetUserPassword;
using QuoteFlow.Application.Users.Commands.UpdateUser;
using QuoteFlow.Application.Users.Common;
using QuoteFlow.Application.Users.Queries.GetUserById;
using QuoteFlow.Application.Users.Queries.GetUsers;

namespace QuoteFlow.Api.Controllers;

/// <summary>Gestión de usuarios (sección 4.2). Solo Administrador.</summary>
[Authorize(Policy = Policies.Administrator)]
public sealed class UsersController : ApiControllerBase
{
    /// <summary>Lista y busca usuarios por nombre o correo, con filtros por rol y estado (RF-USR-01).</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] GetUsersQuery query,
        [FromServices] IQueryHandler<GetUsersQuery, PagedResult<UserDto>> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(query, cancellationToken));

    /// <summary>Detalle de un usuario.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(
        Guid id,
        [FromServices] IQueryHandler<GetUserByIdQuery, UserDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetUserByIdQuery(id), cancellationToken));

    /// <summary>Crea un usuario con contraseña temporal (RF-USR-02).</summary>
    [HttpPost]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedResponse>> CreateUser(
        CreateUserCommand command,
        [FromServices] ICommandHandler<CreateUserCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var id = await handler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id }, new CreatedResponse(id));
    }

    /// <summary>Edita nombre, correo y rol (RF-USR-03). Protege al último Administrador activo (RN-31).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        UpdateUserRequest request,
        [FromServices] ICommandHandler<UpdateUserCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new UpdateUserCommand(id, request.FullName, request.Email, request.Role),
            cancellationToken);
        return NoContent();
    }

    /// <summary>Desactiva un usuario y cierra sus sesiones (RF-USR-04).</summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivateUser(
        Guid id,
        [FromServices] ICommandHandler<DeactivateUserCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new DeactivateUserCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Reactiva un usuario (RF-USR-04).</summary>
    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReactivateUser(
        Guid id,
        [FromServices] ICommandHandler<ReactivateUserCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ReactivateUserCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Asigna una contraseña temporal (RF-USR-05).</summary>
    [HttpPost("{id:guid}/reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetPassword(
        Guid id,
        ResetPasswordRequest request,
        [FromServices] ICommandHandler<ResetUserPasswordCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ResetUserPasswordCommand(id, request.TemporaryPassword), cancellationToken);
        return NoContent();
    }
}
