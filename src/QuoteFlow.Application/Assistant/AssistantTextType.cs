namespace QuoteFlow.Application.Assistant;

/// <summary>Texto comercial sobre el que actúa el asistente (sección 11.1).</summary>
public enum AssistantTextType
{
    Introduction = 1,
    ExecutiveSummary = 2,
    Notes = 3,
    CommercialTerms = 4
}

/// <summary>Acción del asistente: redactar desde cero o mejorar el texto existente.</summary>
public enum AssistantAction
{
    Generate = 1,
    Improve = 2
}
