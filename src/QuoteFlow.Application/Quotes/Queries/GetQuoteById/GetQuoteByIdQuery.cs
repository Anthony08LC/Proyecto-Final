using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Quotes.Common;

namespace QuoteFlow.Application.Quotes.Queries.GetQuoteById;

/// <summary>
/// Detalle completo de una cotización (RF-COT-13): encabezado, líneas, textos, desglose, historial
/// de eventos, enlace a la cotización de origen y acciones válidas según el estado y los permisos.
/// Visible para Ventas y Administrador (RN-32). En borradores, líneas y desglose se muestran con el
/// catálogo, la condición VIP y la configuración vigentes (RN-22), sin guardar cambios.
/// </summary>
public sealed record GetQuoteByIdQuery(Guid QuoteId) : IQuery<QuoteDetailDto>;
