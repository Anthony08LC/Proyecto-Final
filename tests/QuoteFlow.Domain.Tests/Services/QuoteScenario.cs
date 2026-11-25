using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Services;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Domain.Tests.Services;

/// <summary>
/// Arma una cotización real con la API pública del dominio (cliente, categoría, productos y
/// líneas) y la calcula con el motor oficial. Así las pruebas recorren el mismo camino que la
/// aplicación, sin depender de miembros internos.
/// </summary>
internal sealed class QuoteScenario
{
    private readonly List<(decimal Price, decimal Quantity)> _lines = [];
    private bool _isVip;
    private SystemSettings _settings = SystemSettings.CreateDefault();

    public static QuoteScenario Create() => new();

    public QuoteScenario WithLine(decimal unitPrice, decimal quantity = 1m)
    {
        _lines.Add((unitPrice, quantity));
        return this;
    }

    public QuoteScenario Vip()
    {
        _isVip = true;
        return this;
    }

    public QuoteScenario WithSettings(SystemSettings settings)
    {
        _settings = settings;
        return this;
    }

    public QuoteBreakdown Calculate() => Build().Breakdown;

    public (Quote Quote, QuoteBreakdown Breakdown) Build()
    {
        var customer = Customer.Create("Cliente de prueba", null, null, null, null, _isVip);
        var category = Category.Create("Equipo tecnológico", null);
        var quote = Quote.Create(customer, Guid.NewGuid());

        // Cada línea usa un producto distinto: un producto no se repite en la cotización (RN-28).
        var index = 0;
        foreach (var (price, quantity) in _lines)
        {
            var product = Product.Create($"Producto {++index}", null, category, Money.Create(price));
            quote.AddLine(product, category, quantity);
        }

        return (quote, QuoteCalculator.Calculate(quote, customer, _settings));
    }
}
