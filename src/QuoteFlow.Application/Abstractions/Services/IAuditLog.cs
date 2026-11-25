using QuoteFlow.Application.Common.Models;

namespace QuoteFlow.Application.Abstractions.Services;

/// <summary>
/// Registro de eventos de auditoría. Los eventos se guardan en la misma transacción
/// que el cambio que describen, al confirmar con <see cref="Persistence.IUnitOfWork"/>.
/// </summary>
public interface IAuditLog
{
    void Record(AuditEntry entry);
}
