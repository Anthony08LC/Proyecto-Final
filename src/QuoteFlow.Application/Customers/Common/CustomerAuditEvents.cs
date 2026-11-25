namespace QuoteFlow.Application.Customers.Common;

/// <summary>Tipos de evento de auditoría del módulo de clientes.</summary>
internal static class CustomerAuditEvents
{
    public const string Created = "CustomerCreated";
    public const string Updated = "CustomerUpdated";
    public const string VipChanged = "CustomerVipChanged";
    public const string Deactivated = "CustomerDeactivated";
    public const string Reactivated = "CustomerReactivated";
}
