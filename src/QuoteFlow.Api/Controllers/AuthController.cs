using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuoteFlow.Api.Security;
using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Users.Commands.ChangePassword;
using QuoteFlow.Application.Users.Commands.Login;
using QuoteFlow.Application.Users.Commands.UpdateProfile;
using QuoteFlow.Application.Users.Common;
using QuoteFlow.Application.Users.Queries.GetCurrentUserProfile;

namespace QuoteFlow.Api.Controllers;

/// <summary>
/// Sesión y perfil del usuario autenticado (sección 4.1). El cierre de sesión (RF-AUT-02) ocurre en
/// el cliente, que descarta el token; no hay refresh tokens.
/// </summary>
public sealed class AuthController : ApiControllerBase
{
    /// <summary>Inicia sesión con correo y contraseña (RF-AUT-01). Limitado a 5 intentos por minuto por IP.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingSetup.LoginPolicy)]
    [ProducesResponseType<LoginResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResult>> Login(
        LoginCommand command,
        [FromServices] ICommandHandler<LoginCommand, LoginResult> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(command, cancellationToken));

    /// <summary>Perfil del usuario autenticado: nombre, correo y rol (RF-AUT-05).</summary>
    [HttpGet("me")]
    [AllowPendingPasswordChange]
    public async Task<ActionResult<UserProfileDto>> GetProfile(
        [FromServices] IQueryHandler<GetCurrentUserProfileQuery, UserProfileDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(new GetCurrentUserProfileQuery(), cancellationToken));

    /// <summary>Cambia el nombre del usuario autenticado (RF-AUT-05).</summary>
    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(
        UpdateProfileCommand command,
        [FromServices] ICommandHandler<UpdateProfileCommand, UserProfileDto> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(command, cancellationToken));

    /// <summary>
    /// Cambia la contraseña propia (RF-AUT-06, RF-AUT-07). Invalida las demás sesiones y devuelve un
    /// token nuevo para la sesión actual. Es lo único permitido con una contraseña temporal.
    /// </summary>
    [HttpPost("change-password")]
    [AllowPendingPasswordChange]
    public async Task<ActionResult<AccessToken>> ChangePassword(
        ChangePasswordCommand command,
        [FromServices] ICommandHandler<ChangePasswordCommand, AccessToken> handler,
        CancellationToken cancellationToken) =>
        Ok(await handler.HandleAsync(command, cancellationToken));
}
