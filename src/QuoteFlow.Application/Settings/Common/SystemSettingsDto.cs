namespace QuoteFlow.Application.Settings.Common;

/// <summary>
/// Configuración del sistema (RF-CFG-01): datos de la empresa, condiciones comerciales por defecto,
/// parámetros comerciales y vigencia. Los porcentajes van de 0 a 100.
/// </summary>
public sealed record SystemSettingsDto(
    string CompanyName,
    string? CompanyLegalId,
    string? CompanyEmail,
    string? CompanyPhone,
    string? CompanyAddress,
    string? DefaultCommercialTerms,
    decimal VatRate,
    decimal Tier1Threshold,
    decimal Tier1DiscountRate,
    decimal Tier2Threshold,
    decimal Tier2DiscountRate,
    decimal VipDiscountRate,
    int ValidityDays,
    DateTimeOffset UpdatedAt);
