using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Application.Settings.Common;

internal static class SettingsMappings
{
    public static SystemSettingsDto ToDto(this SystemSettings settings) =>
        new(
            settings.CompanyName,
            settings.CompanyLegalId,
            settings.CompanyEmail?.Value,
            settings.CompanyPhone?.Value,
            settings.CompanyAddress,
            settings.DefaultCommercialTerms,
            settings.VatPercentage.Value,
            settings.Tier1Threshold.Amount,
            settings.Tier1Discount.Value,
            settings.Tier2Threshold.Amount,
            settings.Tier2Discount.Value,
            settings.VipDiscount.Value,
            settings.ValidityDays,
            settings.UpdatedAt);
}
