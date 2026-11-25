using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Settings.Common;

namespace QuoteFlow.Application.Settings.Queries.GetCommercialParametersImpact;

/// <summary>
/// Informa, antes de guardar, cuántos borradores se recalcularán si se cambian los parámetros
/// comerciales (RF-CFG-03). Solo Administrador.
/// </summary>
public sealed record GetCommercialParametersImpactQuery : IQuery<CommercialParametersImpactDto>;
