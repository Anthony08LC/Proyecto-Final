namespace QuoteFlow.Api.Security;

/// <summary>
/// Marca los endpoints que un usuario con contraseña temporal puede usar antes de cambiarla
/// (RF-AUT-07): el cambio de contraseña y la consulta de su perfil.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowPendingPasswordChangeAttribute : Attribute;
