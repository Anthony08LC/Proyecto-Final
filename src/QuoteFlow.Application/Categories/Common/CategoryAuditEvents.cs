namespace QuoteFlow.Application.Categories.Common;

/// <summary>Tipos de evento de auditoría del módulo de categorías.</summary>
internal static class CategoryAuditEvents
{
    public const string Created = "CategoryCreated";
    public const string Updated = "CategoryUpdated";
    public const string Deactivated = "CategoryDeactivated";
    public const string Reactivated = "CategoryReactivated";
}
