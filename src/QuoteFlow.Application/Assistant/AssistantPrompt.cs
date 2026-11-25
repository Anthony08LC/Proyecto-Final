namespace QuoteFlow.Application.Assistant;

/// <summary>Solicitud al modelo: instrucciones del sistema y mensaje con el contexto de la cotización.</summary>
/// <param name="SystemInstruction">Reglas fijas: tono, formato y restricciones (sección 11.2).</param>
/// <param name="UserMessage">Tarea y contexto mínimo de la cotización.</param>
public sealed record AssistantPrompt(string SystemInstruction, string UserMessage);
