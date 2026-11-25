namespace QuoteFlow.Application.Common.Models;

/// <summary>
/// Evento de auditoría a registrar. La fecha la completa la implementación de
/// <see cref="Abstractions.Services.IAuditLog"/>; el usuario también, salvo que se indique
/// <paramref name="ActorId"/> (por ejemplo, en el inicio de sesión, cuando aún no hay usuario autenticado).
/// </summary>
/// <param name="EventType">Tipo de evento, por ejemplo <c>QuoteIssued</c>.</param>
/// <param name="EntityType">Entidad afectada.</param>
/// <param name="EntityId">Identificador de la entidad; nulo para la configuración y las sesiones.</param>
/// <param name="Detail">Descripción legible (máximo 2000 caracteres); nunca incluye contraseñas.</param>
/// <param name="ActorId">Usuario que realiza la acción, si no es el usuario autenticado.</param>
/// <param name="ActorName">Nombre del usuario indicado en <paramref name="ActorId"/>.</param>
public sealed record AuditEntry(
    string EventType,
    AuditEntityType EntityType,
    Guid? EntityId,
    string Detail,
    Guid? ActorId = null,
    string? ActorName = null)
{
    public const int DetailMaxLength = 2000;
}
