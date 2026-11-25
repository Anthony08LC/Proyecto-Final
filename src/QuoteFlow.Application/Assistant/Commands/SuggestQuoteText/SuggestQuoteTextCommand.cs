using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Assistant.Commands.SuggestQuoteText;

/// <summary>
/// Pide al asistente una sugerencia para uno de los cuatro textos comerciales de un borrador
/// (RF-IA-01, RF-IA-02). Propietario o Administrador. Devuelve la sugerencia <b>sin guardarla</b>:
/// solo al aceptarla se guarda, con <c>UpdateQuoteTextsCommand</c> (sección 11.3).
/// </summary>
/// <param name="CurrentText">
/// Texto a mejorar, tal como está en pantalla (puede tener cambios sin guardar). Si se omite, se usa
/// el texto guardado. Obligatorio, en la práctica, para la acción «mejorar».
/// </param>
public sealed record SuggestQuoteTextCommand(
    Guid QuoteId,
    AssistantTextType? TextType,
    AssistantAction? Action,
    string? CurrentText) : ICommand<QuoteTextSuggestionDto>;

/// <summary>Sugerencia del asistente; el original acompaña a las mejoras para compararlas.</summary>
public sealed record QuoteTextSuggestionDto(
    AssistantTextType TextType,
    AssistantAction Action,
    string Suggestion,
    string? Original,
    int RemainingThisHour);
