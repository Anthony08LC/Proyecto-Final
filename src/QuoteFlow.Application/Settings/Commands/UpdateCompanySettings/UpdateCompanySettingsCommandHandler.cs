using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Formatting;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Security;
using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Application.Settings.Common;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Application.Settings.Commands.UpdateCompanySettings;

internal sealed class UpdateCompanySettingsCommandHandler(
    ISystemSettingsRepository settingsRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<UpdateCompanySettingsCommand>
{
    public async Task HandleAsync(
        UpdateCompanySettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        currentUser.EnsureAdministrator();

        var errors = new ValidationErrors();

        var name = errors.Required(
            "companyName", "El nombre de la empresa", command.CompanyName, SystemSettings.CompanyNameMaxLength);
        var legalId = errors.Optional(
            "companyLegalId", "La identificación", command.CompanyLegalId, SystemSettings.CompanyLegalIdMaxLength);
        var address = errors.Optional(
            "companyAddress", "La dirección", command.CompanyAddress, SystemSettings.CompanyAddressMaxLength);
        var terms = errors.Optional(
            "defaultCommercialTerms",
            "Las condiciones comerciales por defecto",
            command.DefaultCommercialTerms,
            SystemSettings.DefaultCommercialTermsMaxLength);
        var email = errors.Capture("companyEmail", () => Email.CreateOptional(command.CompanyEmail));
        var phone = errors.Capture("companyPhone", () => Phone.CreateOptional(command.CompanyPhone));

        errors.ThrowIfAny();

        var settings = await settingsRepository.GetAsync(cancellationToken);

        var changes = new ChangeLog();
        changes.Track("Nombre", settings.CompanyName, name);
        changes.Track("Identificación", settings.CompanyLegalId, legalId);
        changes.Track("Correo", settings.CompanyEmail?.Value, email?.Value);
        changes.Track("Teléfono", settings.CompanyPhone?.Value, phone?.Value);
        changes.Track("Dirección", settings.CompanyAddress, address);
        changes.Track("Condiciones comerciales por defecto", settings.DefaultCommercialTerms, terms);

        if (!changes.HasChanges)
        {
            return;
        }

        settings.UpdateCompanyInfo(name, legalId, email, phone, address, terms);

        auditLog.Record(new AuditEntry(
            SettingsAuditEvents.Updated,
            AuditEntityType.SystemSettings,
            EntityId: null,
            DisplayFormat.Truncate(
                $"Datos de la empresa actualizados. {changes}.",
                AuditEntry.DetailMaxLength)));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
