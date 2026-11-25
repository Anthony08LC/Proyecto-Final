namespace QuoteFlow.Application.Settings.Common;

/// <summary>
/// Efecto de cambiar los parámetros comerciales: cuántos borradores usarán los nuevos valores.
/// Las cotizaciones emitidas no se ven afectadas (RN-26).
/// </summary>
public sealed record CommercialParametersImpactDto(int DraftQuotesToRecalculate);
