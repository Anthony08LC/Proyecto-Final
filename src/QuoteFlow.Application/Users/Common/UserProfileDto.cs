using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Application.Users.Common;

/// <summary>Perfil del usuario autenticado (RF-AUT-05).</summary>
public sealed record UserProfileDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool MustChangePassword);
