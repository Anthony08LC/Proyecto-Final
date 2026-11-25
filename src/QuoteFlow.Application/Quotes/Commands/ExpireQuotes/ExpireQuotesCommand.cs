using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Quotes.Commands.ExpireQuotes;

/// <summary>
/// Vence toda cotización Emitida cuya fecha de vencimiento ya pasó en hora de Costa Rica
/// (RF-COT-11). Lo ejecuta el proceso diario con el usuario de sistema como usuario actual; no se
/// expone en la API. Es idempotente (solo toma cotizaciones aún Emitidas) y, si el servidor estuvo
/// detenido, vence todo lo pendiente. Devuelve la cantidad de cotizaciones vencidas.
/// </summary>
public sealed record ExpireQuotesCommand : ICommand<int>;
