using Microsoft.AspNetCore.Mvc;

namespace QuoteFlow.Api.Controllers;

/// <summary>
/// Base de los controladores: rutas bajo <c>/api</c>, JSON y respuestas de error en formato
/// <i>ProblemDetails</i>. Los controladores solo traducen HTTP a comandos y consultas de la capa de
/// aplicación; las reglas y los permisos finos (propiedad, RN-32) viven en los casos de uso.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public abstract class ApiControllerBase : ControllerBase;

/// <summary>Respuesta de una creación: el identificador del recurso nuevo.</summary>
public sealed record CreatedResponse(Guid Id);
