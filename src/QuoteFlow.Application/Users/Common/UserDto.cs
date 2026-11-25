using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Users.Common;

/// <summary>Datos de un usuario para su gestión (RF-USR-01). Nunca incluye la contraseña.</summary>
public sealed record UserDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    bool MustChangePassword,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
