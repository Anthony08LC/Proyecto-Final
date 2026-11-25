using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Abstractions.Services;

/// <summary>Usuario autenticado que ejecuta la operación actual.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid UserId { get; }

    string FullName { get; }

    UserRole Role { get; }

    bool IsAdministrator => Role == UserRole.Administrator;
}
