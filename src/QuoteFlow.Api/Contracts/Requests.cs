using QuoteFlow.Application.Assistant;
using QuoteFlow.Domain.Enums;

namespace QuoteFlow.Api.Contracts;

// Cuerpos de las solicitudes cuyo identificador viene en la ruta. Los demás endpoints reciben
// directamente el comando de la capa de aplicación.

/// <summary>Datos editables de una categoría.</summary>
public sealed record UpdateCategoryRequest(string? Name, string? Description);

/// <summary>Datos editables de un cliente. La condición VIP tiene sus propios endpoints.</summary>
public sealed record UpdateCustomerRequest(
    string? Name,
    string? Company,
    string? Email,
    string? Phone,
    string? Notes);

/// <summary>Datos editables de un producto, incluido su precio.</summary>
public sealed record UpdateProductRequest(
    string? Name,
    string? Description,
    Guid CategoryId,
    decimal Price);

/// <summary>Datos editables de un usuario.</summary>
public sealed record UpdateUserRequest(string? FullName, string? Email, UserRole Role);

/// <summary>Contraseña temporal asignada por un Administrador.</summary>
public sealed record ResetPasswordRequest(string? TemporaryPassword);

/// <summary>Producto y cantidad a agregar a un borrador.</summary>
public sealed record AddQuoteLineRequest(Guid ProductId, decimal Quantity);

/// <summary>Nueva cantidad de una línea.</summary>
public sealed record ChangeQuantityRequest(decimal Quantity);

/// <summary>Nuevo cliente del borrador.</summary>
public sealed record ChangeQuoteCustomerRequest(Guid CustomerId);

/// <summary>Los cuatro textos comerciales; un valor vacío deja el campo sin texto.</summary>
public sealed record UpdateQuoteTextsRequest(
    string? Introduction,
    string? ExecutiveSummary,
    string? Notes,
    string? CommercialTerms);

/// <summary>
/// Solicitud al asistente: texto (<c>Introduction</c>, <c>ExecutiveSummary</c>, <c>Notes</c>,
/// <c>CommercialTerms</c>), acción (<c>Generate</c>, <c>Improve</c>) y, para mejorar, el texto en pantalla.
/// </summary>
public sealed record SuggestQuoteTextRequest(
    AssistantTextType? TextType,
    AssistantAction? Action,
    string? CurrentText);

/// <summary>Motivo de un cambio de estado: opcional al rechazar; obligatorio al cancelar una emitida.</summary>
public sealed record ReasonRequest(string? Reason);
