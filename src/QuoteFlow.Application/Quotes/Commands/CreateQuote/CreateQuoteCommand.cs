using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Quotes.Commands.CreateQuote;

/// <summary>
/// Crea un borrador para un cliente activo (RF-COT-01, RN-33). El propietario es el usuario
/// que lo crea y las condiciones comerciales toman el texto por defecto de la configuración.
/// No tiene número ni vigencia. Devuelve el id creado.
/// </summary>
public sealed record CreateQuoteCommand(Guid CustomerId) : ICommand<Guid>;
