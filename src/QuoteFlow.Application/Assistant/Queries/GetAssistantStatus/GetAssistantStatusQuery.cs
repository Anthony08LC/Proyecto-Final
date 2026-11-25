using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Assistant.Queries.GetAssistantStatus;

/// <summary>Disponibilidad del asistente y cupo del usuario actual (RF-IA-03, RNF-SEG-12).</summary>
public sealed record GetAssistantStatusQuery : IQuery<AssistantStatusDto>;

/// <param name="IsAvailable">Falso si Gemini no está configurado: el frontend deshabilita los botones.</param>
/// <param name="Message">Explicación para el usuario cuando no está disponible.</param>
/// <param name="RequestsPerHour">Límite de solicitudes por hora y por usuario.</param>
/// <param name="RemainingThisHour">Solicitudes que le quedan al usuario en la hora actual.</param>
public sealed record AssistantStatusDto(
    bool IsAvailable,
    string? Message,
    int RequestsPerHour,
    int RemainingThisHour);
