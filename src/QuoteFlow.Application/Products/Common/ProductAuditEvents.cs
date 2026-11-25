namespace QuoteFlow.Application.Products.Common;

/// <summary>Tipos de evento de auditoría del módulo de productos.</summary>
internal static class ProductAuditEvents
{
    public const string Created = "ProductCreated";
    public const string Updated = "ProductUpdated";
    public const string PriceChanged = "ProductPriceChanged";
    public const string Deactivated = "ProductDeactivated";
    public const string Reactivated = "ProductReactivated";
}
