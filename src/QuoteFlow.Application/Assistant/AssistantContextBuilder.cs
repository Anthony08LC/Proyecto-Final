using System.Text;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Common.Formatting;
using QuoteFlow.Application.Customers.Common;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Assistant;

/// <summary>
/// Arma el contexto mínimo que se envía al modelo (sección 11.3, paso 4; RNF-SEG-11): nombre de la
/// empresa, nombre y empresa del cliente, condición VIP, líneas y el desglose calculado por el motor
/// (los montos nunca los calcula el modelo, sección 11.2). No incluye correos, teléfonos, cédulas,
/// direcciones ni notas internas del cliente; los textos libres pasan por <see cref="SensitiveDataFilter"/>.
/// </summary>
internal sealed class AssistantContextBuilder(
    ICustomerRepository customers,
    ISystemSettingsRepository settingsRepository,
    QuoteDraftCalculator calculator)
{
    /// <summary>Máximo de caracteres de cada texto libre incluido como referencia.</summary>
    private const int ReferenceTextMaxLength = 1500;

    /// <summary>Máximo de caracteres de la descripción de un producto.</summary>
    private const int DescriptionMaxLength = 300;

    /// <summary>Máximo de líneas detalladas; las demás se resumen.</summary>
    private const int MaxLines = 50;

    /// <summary>
    /// Recalcula el borrador con el catálogo y la política vigentes (RN-22) y describe la cotización.
    /// El recálculo modifica la entidad en memoria: el llamador debe descartar esos cambios.
    /// </summary>
    public async Task<string> BuildAsync(Quote quote, AssistantTextType textType, CancellationToken cancellationToken)
    {
        var customer = await customers.GetRequiredAsync(quote.CustomerId, cancellationToken);
        var settings = await settingsRepository.GetAsync(cancellationToken);
        await calculator.RecalculateAsync(quote, customer, cancellationToken);

        var context = new StringBuilder();
        context.AppendLine($"Empresa que cotiza: {settings.CompanyName}");
        context.AppendLine($"Cliente: {customer.Name}");
        if (!string.IsNullOrWhiteSpace(customer.Company))
        {
            context.AppendLine($"Empresa del cliente: {customer.Company}");
        }
        context.AppendLine($"Cliente VIP: {DisplayFormat.YesNo(customer.IsVip)}");
        context.AppendLine($"Vigencia de la oferta: {settings.ValidityDays} días a partir de la emisión");

        AppendLines(context, quote);
        AppendTotals(context, quote);
        AppendReferenceTexts(context, quote, settings, textType);

        return context.ToString();
    }

    private static void AppendLines(StringBuilder context, Quote quote)
    {
        context.AppendLine();
        if (quote.Lines.Count == 0)
        {
            context.AppendLine("Productos: la cotización aún no tiene productos.");
            return;
        }

        context.AppendLine("Productos (cantidad × precio unitario = subtotal de la línea):");
        foreach (var line in quote.Lines.OrderBy(l => l.LineNumber).Take(MaxLines))
        {
            context.Append($"- {line.ProductName} [{line.CategoryName}]: {DisplayFormat.Quantity(line.Quantity)} × "
                + $"{DisplayFormat.Colones(line.UnitPrice.Amount)} = {DisplayFormat.Colones(line.CalculateSubtotal().Amount)}");

            if (!string.IsNullOrWhiteSpace(line.ProductDescription))
            {
                context.Append($". {Reference(line.ProductDescription, DescriptionMaxLength)}");
            }
            context.AppendLine();
        }

        if (quote.Lines.Count > MaxLines)
        {
            context.AppendLine($"- … y {quote.Lines.Count - MaxLines} productos más.");
        }
    }

    /// <summary>Montos del motor de cálculo, estimados hasta la emisión (RN-22).</summary>
    private static void AppendTotals(StringBuilder context, Quote quote)
    {
        var breakdown = quote.Breakdown;
        context.AppendLine();
        context.AppendLine("Montos calculados por el sistema (estimados hasta emitir; no deben modificarse):");
        context.AppendLine($"- Subtotal: {DisplayFormat.Colones(breakdown.Subtotal.Amount)}");
        if (breakdown.DiscountAmount.Amount > 0)
        {
            context.AppendLine($"- Descuento ({DisplayFormat.Percent(breakdown.DiscountRate.Value)}): "
                + $"{DisplayFormat.Colones(breakdown.DiscountAmount.Amount)}");
        }
        context.AppendLine($"- IVA ({DisplayFormat.Percent(breakdown.VatRate.Value)}): {DisplayFormat.Colones(breakdown.VatAmount.Amount)}");
        context.AppendLine($"- Total: {DisplayFormat.Colones(breakdown.Total.Amount)}");
    }

    /// <summary>Los demás textos ya guardados, para mantener la coherencia del documento.</summary>
    private static void AppendReferenceTexts(
        StringBuilder context,
        Quote quote,
        SystemSettings settings,
        AssistantTextType textType)
    {
        var references = new List<(string Label, string? Text)>
        {
            ("Introducción", textType == AssistantTextType.Introduction ? null : quote.Introduction),
            ("Resumen ejecutivo", textType == AssistantTextType.ExecutiveSummary ? null : quote.ExecutiveSummary),
            ("Observaciones", textType == AssistantTextType.Notes ? null : quote.Notes),
            ("Condiciones comerciales", textType == AssistantTextType.CommercialTerms ? null : quote.CommercialTerms)
        };

        if (textType == AssistantTextType.CommercialTerms)
        {
            references.Add(("Condiciones comerciales predeterminadas de la empresa", settings.DefaultCommercialTerms));
        }

        var present = references.Where(r => !string.IsNullOrWhiteSpace(r.Text)).ToList();
        if (present.Count == 0)
        {
            return;
        }

        context.AppendLine();
        context.AppendLine("Otros textos de la cotización (solo como referencia):");
        foreach (var (label, text) in present)
        {
            context.AppendLine($"{label}: {Reference(text!, ReferenceTextMaxLength)}");
        }
    }

    private static string Reference(string text, int maxLength) =>
        DisplayFormat.Truncate(SensitiveDataFilter.Redact(text.Trim()), maxLength);
}
