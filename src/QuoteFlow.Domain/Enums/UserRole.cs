namespace QuoteFlow.Domain.Enums;

/// <summary>Rol que determina los permisos de un usuario en el sistema.</summary>
public enum UserRole
{
    /// <summary>Administrador: gestión completa del sistema.</summary>
    Administrator = 1,

    /// <summary>Ventas: creación y gestión de cotizaciones.</summary>
    Sales = 2
}
