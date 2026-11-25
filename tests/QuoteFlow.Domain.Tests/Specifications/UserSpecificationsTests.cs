using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.Specifications;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Domain.Tests.Specifications;

/// <summary>
/// Quién cuenta para la regla del último Administrador activo (RN-31). Es el mismo predicado que
/// el repositorio traduce a SQL en <c>CountActiveAdministratorsAsync</c>.
/// </summary>
public sealed class UserSpecificationsTests
{
    private static readonly Func<User, bool> CountsAsActiveAdministrator =
        UserSpecifications.CountsAsActiveAdministrator.Compile();

    private static User NewUser(string email, UserRole role) =>
        User.Create("Usuario de prueba", Email.Create(email), "hash", role);

    [Fact(DisplayName = "RN-31 · El usuario de sistema no cuenta como Administrador activo")]
    public void SystemUserIsExcluded()
    {
        var system = User.CreateSystem("Sistema", Email.Create("sistema@quoteflow.local"));

        // Precondición: el usuario de sistema sí tiene rol Administrador y está activo.
        Assert.Equal(UserRole.Administrator, system.Role);
        Assert.True(system.IsActive);

        Assert.False(CountsAsActiveAdministrator(system));
    }

    [Fact(DisplayName = "RN-31 · Con el sistema y un solo Administrador real, ese Administrador es el último")]
    public void SystemUserDoesNotHideTheLastAdministrator()
    {
        var users = new[]
        {
            User.CreateSystem("Sistema", Email.Create("sistema@quoteflow.local")),
            NewUser("admin@ejemplo.test", UserRole.Administrator),
            NewUser("ventas@ejemplo.test", UserRole.Sales),
        };

        Assert.Equal(1, users.Count(CountsAsActiveAdministrator));
    }

    [Fact(DisplayName = "RN-31 · Solo cuentan los Administradores activos")]
    public void OnlyActiveAdministratorsCount()
    {
        var active = NewUser("activo@ejemplo.test", UserRole.Administrator);
        var inactive = NewUser("inactivo@ejemplo.test", UserRole.Administrator);
        inactive.Deactivate();
        var sales = NewUser("ventas@ejemplo.test", UserRole.Sales);

        Assert.True(CountsAsActiveAdministrator(active));
        Assert.False(CountsAsActiveAdministrator(inactive));
        Assert.False(CountsAsActiveAdministrator(sales));
    }
}
