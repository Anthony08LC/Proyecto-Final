using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Settings.Common;

namespace QuoteFlow.Application.Settings.Commands.UpdateCommercialParameters;

/// <summary>
/// Actualiza el IVA, los tramos de descuento, el descuento VIP y la vigencia (RF-CFG-03).
/// Solo Administrador. Valida cada regla de RN-27 con un mensaje específico. El cambio afecta
/// solo a los borradores, nunca a las cotizaciones emitidas (RN-26). La auditoría registra el
/// valor anterior y el nuevo de cada parámetro modificado.
/// </summary>
public sealed record UpdateCommercialParametersCommand(
    decimal VatRate,
    decimal Tier1Threshold,
    decimal Tier1DiscountRate,
    decimal Tier2Threshold,
    decimal Tier2DiscountRate,
    decimal VipDiscountRate,
    int ValidityDays) : ICommand<CommercialParametersImpactDto>;
