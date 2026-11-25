using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using QuoteFlow.Api.Controllers;
using QuoteFlow.Api.Tests.Infrastructure;
using QuoteFlow.Application.Categories.Common;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Customers.Common;
using QuoteFlow.Application.Products.Common;
using QuoteFlow.Application.Quotes.Commands.IssueQuote;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Api.Tests;

/// <summary>Emisión (sección 9.2): consecutivo COT-AAAA-000001 y congelamiento de los datos.</summary>
[Collection(ApiCollection.Name)]
public sealed partial class QuoteIssuingTests(QuoteFlowApiFactory factory)
{
    [Fact]
    public async Task Emisiones_sucesivas_reciben_numeros_consecutivos()
    {
        using var admin = factory.CreateClient(factory.AdminToken);
        using var sales = factory.CreateClient(factory.SalesToken);
        var customerId = await CreateCustomerAsync(admin);
        var productId = await CreateProductAsync(admin, price: 50_000m);

        var first = await IssueAsync(sales, await CreateDraftAsync(sales, customerId, productId, 1m));
        var second = await IssueAsync(sales, await CreateDraftAsync(sales, customerId, productId, 1m));

        var firstMatch = NumberPattern().Match(first.Number);
        var secondMatch = NumberPattern().Match(second.Number);
        Assert.True(firstMatch.Success, first.Number);
        Assert.True(secondMatch.Success, second.Number);
        Assert.Equal(firstMatch.Groups["year"].Value, secondMatch.Groups["year"].Value);
        Assert.Equal(
            int.Parse(firstMatch.Groups["seq"].Value, CultureInfo.InvariantCulture) + 1,
            int.Parse(secondMatch.Groups["seq"].Value, CultureInfo.InvariantCulture));
        Assert.True(second.ValidUntil >= DateOnly.FromDateTime(second.IssuedAt.UtcDateTime));
    }

    [Fact]
    public async Task Cotizacion_emitida_conserva_precio_y_totales_ante_un_cambio_de_precio()
    {
        using var admin = factory.CreateClient(factory.AdminToken);
        using var sales = factory.CreateClient(factory.SalesToken);
        var customerId = await CreateCustomerAsync(admin);
        var categoryId = await GetCategoryIdAsync(admin);
        var productName = $"Portátil congelado {Guid.NewGuid():N}";
        var productId = await CreateProductAsync(admin, price: 100_000m, categoryId, productName);

        var issuedId = await CreateDraftAsync(sales, customerId, productId, 2m);
        var issue = await IssueAsync(sales, issuedId);
        var before = await GetQuoteAsync(sales, issuedId);

        // El Administrador cambia el precio del producto después de la emisión.
        var update = await admin.PutAsJsonAsync($"/api/products/{productId}",
            new { name = productName, description = (string?)null, categoryId, price = 150_000m }, TestJson.Options);
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var product = await admin.GetFromJsonAsync<ProductDto>($"/api/products/{productId}", TestJson.Options);
        Assert.Equal(150_000m, product!.Price);

        // La emitida no cambia: precio unitario, línea, desglose y número quedan congelados.
        var after = await GetQuoteAsync(sales, issuedId);
        Assert.Equal(QuoteStatus.Issued, after.Status);
        Assert.Equal(issue.Number, after.Number);
        var line = Assert.Single(after.Lines);
        Assert.Equal(100_000m, line.UnitPrice);
        Assert.Equal(200_000m, line.LineTotal);
        Assert.Equal(before.Breakdown, after.Breakdown);
        Assert.Equal(200_000m, after.Breakdown.Subtotal);
        Assert.False(after.Breakdown.IsEstimate);

        // En cambio, un borrador nuevo con el mismo producto ya usa el precio vigente.
        var draftId = await CreateDraftAsync(sales, customerId, productId, 2m);
        var draft = await GetQuoteAsync(sales, draftId);
        Assert.Equal(150_000m, Assert.Single(draft.Lines).UnitPrice);
        Assert.Equal(300_000m, draft.Breakdown.Subtotal);
    }

    [GeneratedRegex(@"^COT-(?<year>\d{4})-(?<seq>\d{6})$")]
    private static partial Regex NumberPattern();

    private static async Task<Guid> GetCategoryIdAsync(HttpClient admin)
    {
        var categories = await admin.GetFromJsonAsync<PagedResult<CategoryDto>>("/api/categories", TestJson.Options);
        return categories!.Items.First(c => c.IsActive).Id;
    }

    private static async Task<Guid> CreateCustomerAsync(HttpClient admin)
    {
        var response = await admin.PostAsJsonAsync("/api/customers", new
        {
            name = $"Cliente {Guid.NewGuid():N}",
            company = "Empresa de Pruebas S.A.",
            email = $"cliente-{Guid.NewGuid():N}@example.com",
            phone = "8888-8888",
            notes = (string?)null,
            isVip = false
        }, TestJson.Options);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CustomerSaveResult>(TestJson.Options))!.CustomerId;
    }

    private static async Task<Guid> CreateProductAsync(
        HttpClient admin,
        decimal price,
        Guid? categoryId = null,
        string? name = null)
    {
        var response = await admin.PostAsJsonAsync("/api/products", new
        {
            name = name ?? $"Producto {Guid.NewGuid():N}",
            description = "Producto de prueba",
            categoryId = categoryId ?? await GetCategoryIdAsync(admin),
            price
        }, TestJson.Options);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreatedResponse>(TestJson.Options))!.Id;
    }

    private static async Task<Guid> CreateDraftAsync(HttpClient client, Guid customerId, Guid productId, decimal quantity)
    {
        var created = await client.PostAsJsonAsync("/api/quotes", new { customerId }, TestJson.Options);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var quoteId = (await created.Content.ReadFromJsonAsync<CreatedResponse>(TestJson.Options))!.Id;

        var line = await client.PostAsJsonAsync($"/api/quotes/{quoteId}/lines", new { productId, quantity }, TestJson.Options);
        Assert.Equal(HttpStatusCode.OK, line.StatusCode);

        return quoteId;
    }

    private static async Task<IssueQuoteResult> IssueAsync(HttpClient client, Guid quoteId)
    {
        var response = await client.PostAsync($"/api/quotes/{quoteId}/issue", content: null);
        Assert.True(response.StatusCode == HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<IssueQuoteResult>(TestJson.Options))!;
    }

    private static async Task<QuoteDetailDto> GetQuoteAsync(HttpClient client, Guid quoteId) =>
        (await client.GetFromJsonAsync<QuoteDetailDto>($"/api/quotes/{quoteId}", TestJson.Options))!;
}
