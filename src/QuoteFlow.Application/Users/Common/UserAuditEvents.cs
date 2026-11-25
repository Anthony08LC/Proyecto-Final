namespace QuoteFlow.Application.Users.Common;

/// <summary>Tipos de evento de auditoría de usuarios.</summary>
internal static class UserAuditEvents
{
    public const string Created = "UserCreated";
    public const string Updated = "UserUpdated";
    public const string RoleChanged = "UserRoleChanged";
    public const string Deactivated = "UserDeactivated";
    public const string Reactivated = "UserReactivated";
    public const string PasswordChanged = "UserPasswordChanged";
    public const string PasswordReset = "UserPasswordReset";
}
