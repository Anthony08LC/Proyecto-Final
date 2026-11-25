using QuoteFlow.Domain.Common;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.Exceptions;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Domain.Entities;

/// <summary>
/// Usuario del sistema. El <see cref="SecurityStamp"/> se regenera al cambiar la contraseña
/// o el rol, o al desactivar, para invalidar las sesiones existentes.
/// La unicidad del correo (RN-30) y la regla de al menos un Administrador activo (RN-31)
/// se validan en la capa de aplicación.
/// </summary>
public sealed class User : AuditableEntity
{
    public const int FullNameMaxLength = 150;
    public const int PasswordHashMaxLength = 255;

    public string FullName { get; private set; }

    public Email Email { get; private set; }

    /// <summary>Hash de la contraseña; nulo solo para el usuario de sistema, que no inicia sesión.</summary>
    public string? PasswordHash { get; private set; }

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public bool MustChangePassword { get; private set; }

    public Guid SecurityStamp { get; private set; }

    /// <summary>
    /// Usuario de sistema: ejecuta los procesos automáticos (por ejemplo, el vencimiento).
    /// Nunca inicia sesión, no aparece en los listados y no se puede modificar.
    /// </summary>
    public bool IsSystem { get; private set; }

    private User(string fullName, Email email, string? passwordHash, UserRole role, bool isSystem)
    {
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        IsSystem = isSystem;
        MustChangePassword = !isSystem;
        SecurityStamp = Guid.NewGuid();
    }

    /// <summary>
    /// Crea un usuario activo con contraseña temporal: deberá cambiarla en su primer inicio de sesión.
    /// </summary>
    public static User Create(string? fullName, Email email, string? passwordHash, UserRole role)
    {
        EnsureEmail(email);
        EnsureValidRole(role);

        return new User(
            Guard.Required(fullName, FullNameMaxLength, "nombre completo"),
            email,
            Guard.Required(passwordHash, PasswordHashMaxLength, "hash de la contraseña"),
            role,
            isSystem: false);
    }

    /// <summary>Crea el usuario de sistema, sin contraseña (datos iniciales, sección 9.7).</summary>
    public static User CreateSystem(string? fullName, Email email)
    {
        EnsureEmail(email);

        return new User(
            Guard.Required(fullName, FullNameMaxLength, "nombre completo"),
            email,
            passwordHash: null,
            UserRole.Administrator,
            isSystem: true);
    }

    public void ChangeFullName(string? fullName)
    {
        EnsureNotSystem();
        FullName = Guard.Required(fullName, FullNameMaxLength, "nombre completo");
    }

    public void ChangeEmail(Email email)
    {
        EnsureNotSystem();
        EnsureEmail(email);
        Email = email;
    }

    /// <summary>
    /// Cambia la contraseña y regenera el sello de seguridad.
    /// Con <paramref name="isTemporary"/> en <c>true</c> (restablecimiento por un Administrador),
    /// el usuario deberá cambiarla en su próximo inicio de sesión.
    /// </summary>
    public void ChangePassword(string? passwordHash, bool isTemporary = false)
    {
        EnsureNotSystem();
        PasswordHash = Guard.Required(passwordHash, PasswordHashMaxLength, "hash de la contraseña");
        MustChangePassword = isTemporary;
        RegenerateSecurityStamp();
    }

    /// <summary>Cambia el rol y regenera el sello de seguridad.</summary>
    public void ChangeRole(UserRole role)
    {
        EnsureNotSystem();
        EnsureValidRole(role);

        if (Role == role)
        {
            throw new DomainException("El usuario ya tiene el rol indicado.");
        }

        Role = role;
        RegenerateSecurityStamp();
    }

    public void Activate()
    {
        EnsureNotSystem();

        if (IsActive)
        {
            throw new DomainException("El usuario ya está activo.");
        }

        IsActive = true;
    }

    /// <summary>Desactiva el usuario y regenera el sello de seguridad para cerrar sus sesiones.</summary>
    public void Deactivate()
    {
        EnsureNotSystem();

        if (!IsActive)
        {
            throw new DomainException("El usuario ya está inactivo.");
        }

        IsActive = false;
        RegenerateSecurityStamp();
    }

    private void EnsureNotSystem()
    {
        if (IsSystem)
        {
            throw new DomainException("El usuario de sistema no se puede modificar.");
        }
    }

    private void RegenerateSecurityStamp()
    {
        SecurityStamp = Guid.NewGuid();
    }

    private static void EnsureEmail(Email? email)
    {
        if (email is null)
        {
            throw new DomainException("El correo electrónico es obligatorio.");
        }
    }

    private static void EnsureValidRole(UserRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new DomainException("El rol indicado no es válido.");
        }
    }
}
