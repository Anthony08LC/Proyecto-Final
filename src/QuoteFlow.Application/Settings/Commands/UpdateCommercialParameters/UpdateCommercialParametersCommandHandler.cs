using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Formatting;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Application.Settings.Common;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Application.Settings.Commands.UpdateCommercialParameters;

internal sealed class UpdateCommercialParametersCommandHandler(
    ISystemSettingsRepository settingsRepository,
    IQuoteRepository quotes,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<UpdateCommercialParametersCommand, CommercialParametersImpactDto>
{
    public async Task<CommercialParametersImpactDto> HandleAsync(
        UpdateCommercialParametersCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var parameters = Parse(command);
        var settings = await settingsRepository.GetAsync(cancellationToken);

        var changes = new ChangeLog();
        changes.Track("IVA", DisplayFormat.Percent(settings.VatPercentage.Value), DisplayFormat.Percent(parameters.Vat.Value));
        changes.Track("Umbral tramo 1", DisplayFormat.Colones(settings.Tier1Threshold.Amount), DisplayFormat.Colones(parameters.Tier1Threshold.Amount));
        changes.Track("Descuento tramo 1", DisplayFormat.Percent(settings.Tier1Discount.Value), DisplayFormat.Percent(parameters.Tier1Discount.Value));
        changes.Track("Umbral tramo 2", DisplayFormat.Colones(settings.Tier2Threshold.Amount), DisplayFormat.Colones(parameters.Tier2Threshold.Amount));
        changes.Track("Descuento tramo 2", DisplayFormat.Percent(settings.Tier2Discount.Value), DisplayFormat.Percent(parameters.Tier2Discount.Value));
        changes.Track("Descuento VIP", DisplayFormat.Percent(settings.VipDiscount.Value), DisplayFormat.Percent(parameters.VipDiscount.Value));
        changes.Track("Vigencia (días)", settings.ValidityDays.ToString(), parameters.ValidityDays.ToString());

        var drafts = await quotes.CountByStatusAsync(QuoteStatus.Draft, cancellationToken);

        if (!changes.HasChanges)
        {
            return new CommercialParametersImpactDto(drafts);
        }

        settings.UpdateVatPercentage(parameters.Vat);
        settings.UpdateDiscountPolicy(
            parameters.Tier1Threshold,
            parameters.Tier1Discount,
            parameters.Tier2Threshold,
            parameters.Tier2Discount,
            parameters.VipDiscount);
        settings.UpdateValidityDays(parameters.ValidityDays);

        auditLog.Record(new AuditEntry(
            SettingsAuditEvents.Updated,
            AuditEntityType.SystemSettings,
            EntityId: null,
            DisplayFormat.Truncate(
                $"Parámetros comerciales actualizados. {changes}. Borradores afectados: {drafts}.",
                AuditEntry.DetailMaxLength)));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CommercialParametersImpactDto(drafts);
    }

    /// <summary>
    /// Convierte y valida los parámetros, informando cada regla incumplida de RN-27
    /// junto al campo correspondiente.
    /// </summary>
    private static CommercialParameters Parse(UpdateCommercialParametersCommand command)
    {
        var errors = new ValidationErrors();

        var vat = errors.Capture("vatRate", () => Percentage.Create(command.VatRate));
        var tier1Threshold = errors.Capture("tier1Threshold", () => Money.Create(command.Tier1Threshold));
        var tier1Discount = errors.Capture("tier1DiscountRate", () => Percentage.Create(command.Tier1DiscountRate));
        var tier2Threshold = errors.Capture("tier2Threshold", () => Money.Create(command.Tier2Threshold));
        var tier2Discount = errors.Capture("tier2DiscountRate", () => Percentage.Create(command.Tier2DiscountRate));
        var vipDiscount = errors.Capture("vipDiscountRate", () => Percentage.Create(command.VipDiscountRate));

        if (command.ValidityDays < SystemSettings.MinValidityDays
            || command.ValidityDays > SystemSettings.MaxValidityDays)
        {
            errors.Add(
                "validityDays",
                $"La vigencia debe estar entre {SystemSettings.MinValidityDays} y {SystemSettings.MaxValidityDays} días.");
        }

        if (tier1Threshold is not null && tier1Threshold <= Money.Zero)
        {
            errors.Add("tier1Threshold", "El umbral del tramo 1 debe ser mayor que cero.");
        }

        if (tier1Threshold is not null && tier2Threshold is not null && tier2Threshold <= tier1Threshold)
        {
            errors.Add("tier2Threshold", "El umbral del tramo 2 debe ser mayor que el umbral del tramo 1.");
        }

        if (tier1Discount is not null && tier2Discount is not null && tier2Discount < tier1Discount)
        {
            errors.Add(
                "tier2DiscountRate",
                "El descuento del tramo 2 debe ser mayor o igual que el descuento del tramo 1.");
        }

        if (tier2Discount is not null && vipDiscount is not null
            && tier2Discount.Value + vipDiscount.Value > Percentage.Max)
        {
            errors.Add(
                "vipDiscountRate",
                $"La suma del descuento del tramo 2 y el descuento VIP no puede superar {Percentage.Max:0} %.");
        }

        errors.ThrowIfAny();

        return new CommercialParameters(
            vat!,
            tier1Threshold!,
            tier1Discount!,
            tier2Threshold!,
            tier2Discount!,
            vipDiscount!,
            command.ValidityDays);
    }

    private sealed record CommercialParameters(
        Percentage Vat,
        Money Tier1Threshold,
        Percentage Tier1Discount,
        Money Tier2Threshold,
        Percentage Tier2Discount,
        Percentage VipDiscount,
        int ValidityDays);
}
