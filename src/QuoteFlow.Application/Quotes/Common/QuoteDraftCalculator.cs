using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Quotes.Common;

/// <summary>
/// Mantiene los borradores al día (RN-22): actualiza las líneas con el catálogo vigente y
/// recalcula el desglose con la condición VIP del cliente y la configuración vigentes.
/// El cálculo lo hace siempre el motor del dominio (RNF-DAT-02).
/// </summary>
internal sealed class QuoteDraftCalculator(
    IProductRepository products,
    ICategoryRepository categories,
    ISystemSettingsRepository settingsRepository)
{
    /// <summary>Carga los productos indicados y sus categorías.</summary>
    public async Task<QuoteCatalog> LoadCatalogAsync(
        IEnumerable<Guid> productIds,
        CancellationToken cancellationToken)
    {
        var ids = productIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            return QuoteCatalog.Empty;
        }

        var productList = await products.GetByIdsAsync(ids, cancellationToken);
        var categoryIds = productList.Select(p => p.CategoryId).Distinct().ToList();
        var categoryList = categoryIds.Count == 0
            ? []
            : await categories.GetByIdsAsync(categoryIds, cancellationToken);

        return new QuoteCatalog(productList, categoryList);
    }

    public Task<QuoteCatalog> LoadCatalogAsync(Quote quote, CancellationToken cancellationToken) =>
        LoadCatalogAsync(quote.Lines.Select(l => l.ProductId), cancellationToken);

    /// <summary>
    /// Actualiza las líneas del borrador con el catálogo vigente y recalcula su desglose.
    /// Devuelve el catálogo cargado para que el llamador pueda reutilizarlo.
    /// </summary>
    public async Task<QuoteCatalog> RecalculateAsync(
        Quote quote,
        Customer customer,
        CancellationToken cancellationToken)
    {
        var catalog = await LoadCatalogAsync(quote, cancellationToken);
        var settings = await settingsRepository.GetAsync(cancellationToken);

        quote.RefreshCatalog(catalog.Products, catalog.Categories);
        quote.Recalculate(customer, settings);

        return catalog;
    }
}
