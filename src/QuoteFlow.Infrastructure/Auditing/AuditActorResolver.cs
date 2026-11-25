using Microsoft.EntityFrameworkCore;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Infrastructure.Persistence;

namespace QuoteFlow.Infrastructure.Auditing;

/// <summary>Usuario al que se atribuye un cambio o un evento de auditoría.</summary>
internal sealed record AuditActor(Guid UserId, string FullName);

/// <summary>
/// Determina quién realiza los cambios: el usuario autenticado o, en los procesos automáticos
/// sin sesión (por ejemplo, el vencimiento diario: RF-COT-11), el usuario de sistema.
/// </summary>
internal sealed class AuditActorResolver(ApplicationDbContext dbContext, ICurrentUser currentUser)
{
    private AuditActor? _systemUser;

    /// <summary>Usuario autenticado, o el usuario de sistema si no hay sesión.</summary>
    public async Task<AuditActor> GetActorAsync(CancellationToken cancellationToken) =>
        GetAuthenticatedActor() ?? await GetSystemUserAsync(cancellationToken);

    public AuditActor? GetAuthenticatedActor() =>
        currentUser.IsAuthenticated ? new AuditActor(currentUser.UserId, currentUser.FullName) : null;

    public async Task<AuditActor> GetSystemUserAsync(CancellationToken cancellationToken)
    {
        return _systemUser ??= await dbContext.Users
            .AsNoTracking()
            .Where(u => u.IsSystem)
            .Select(u => new AuditActor(u.Id, u.FullName))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "No existe el usuario de sistema. Ejecute la carga de datos iniciales.");
    }
}
