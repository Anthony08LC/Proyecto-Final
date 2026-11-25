using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Assistant;
using QuoteFlow.Application.Common.Exceptions;

namespace QuoteFlow.Infrastructure.Assistant;

/// <summary>
/// Asistente «no disponible»: se usa cuando no hay clave de Gemini configurada (RF-IA-03). La
/// aplicación arranca y funciona; solo las funciones de redacción asistida quedan deshabilitadas.
/// </summary>
internal sealed class UnavailableTextAssistant : ITextAssistant
{
    public bool IsAvailable => false;

    public Task<string> CompleteAsync(AssistantPrompt prompt, CancellationToken cancellationToken = default) =>
        throw AssistantUnavailableException.NotConfigured();
}
