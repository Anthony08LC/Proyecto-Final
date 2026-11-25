namespace QuoteFlow.Application.Common.Models;

/// <summary>Tipo de entidad afectada por un evento de auditoría.</summary>
public enum AuditEntityType
{
    User = 1,
    Customer = 2,
    Category = 3,
    Product = 4,
    Quote = 5,
    SystemSettings = 6,
    Session = 7
}
