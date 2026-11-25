using System.Net;
using System.Net.Http.Json;
using QuoteFlow.Api.Tests.Infrastructure;
using QuoteFlow.Application.Users.Commands.Login;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Api.Tests;

/// <summary>Autenticación y autorización (sección 9.2): login, 401 y 403.</summary>
[Collection(ApiCollection.Name)]
public sealed class AuthTests(QuoteFlowApiFactory factory)
{
    [Fact]
    public async Task Login_con_credenciales_correctas_devuelve_token_y_perfil()
    {
        using var client = factory.CreateClient(token: null);

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = QuoteFlowApiFactory.SalesEmail, password = QuoteFlowApiFactory.Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<LoginResult>(TestJson.Options);
        Assert.False(string.IsNullOrWhiteSpace(result!.Token));
        Assert.True(result.ExpiresAt > DateTimeOffset.UtcNow);
        Assert.Equal(QuoteFlowApiFactory.SalesEmail, result.User.Email);
        Assert.Equal(UserRole.Sales, result.User.Role);
        Assert.False(result.User.MustChangePassword);

        // El token recién emitido abre los endpoints protegidos.
        using var authenticated = factory.CreateClient(result.Token);
        var me = await authenticated.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Theory]
    [InlineData(QuoteFlowApiFactory.SalesEmail, "contraseña-incorrecta")]
    [InlineData("no-existe@pruebas.quoteflow.local", QuoteFlowApiFactory.Password)]
    public async Task Login_incorrecto_responde_401_sin_token_con_mensaje_generico(string email, string password)
    {
        using var client = factory.CreateClient(token: null);

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"token\"", body, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("/api/auth/me")]
    [InlineData("/api/quotes")]
    [InlineData("/api/users")]
    public async Task Sin_token_responde_401(string url)
    {
        using var client = factory.CreateClient(token: null);

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Token_invalido_responde_401()
    {
        using var client = factory.CreateClient("token.que.no.es.valido");

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public static TheoryData<string, string, object?> AdministratorEndpoints => new()
    {
        { "GET", "/api/users", null },
        { "POST", "/api/users", new { fullName = "Intruso", email = "intruso@pruebas.quoteflow.local", role = "Sales", temporaryPassword = "Temporal-2026!" } },
        { "POST", "/api/categories", new { name = "Categoría de Ventas", description = (string?)null } },
        { "POST", "/api/products", new { name = "Producto de Ventas", description = (string?)null, categoryId = Guid.NewGuid(), price = 1000m } },
        { "PUT", $"/api/products/{Guid.NewGuid()}", new { name = "Cambio", description = (string?)null, categoryId = Guid.NewGuid(), price = 1m } },
        { "GET", "/api/settings", null },
        { "GET", "/api/audit-events", null }
    };

    [Theory]
    [MemberData(nameof(AdministratorEndpoints))]
    public async Task Ventas_recibe_403_en_funciones_de_Administrador(string method, string url, object? body)
    {
        using var client = factory.CreateClient(factory.SalesToken);
        using var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: TestJson.Options);
        }

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Administrador_si_accede_a_la_gestion_de_usuarios()
    {
        using var client = factory.CreateClient(factory.AdminToken);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
