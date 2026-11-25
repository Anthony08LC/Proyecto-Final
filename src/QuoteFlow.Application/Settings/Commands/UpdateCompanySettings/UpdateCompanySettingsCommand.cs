using QuoteFlow.Application.Abstractions.Messaging;

namespace QuoteFlow.Application.Settings.Commands.UpdateCompanySettings;

/// <summary>
/// Actualiza los datos de la empresa y las condiciones comerciales por defecto (RF-CFG-02).
/// Solo Administrador. El nombre es obligatorio; correo y teléfono se validan igual que en
/// los clientes. La auditoría registra los valores anteriores y los nuevos.
/// </summary>
public sealed record UpdateCompanySettingsCommand(
    string? CompanyName,
    string? CompanyLegalId,
    string? CompanyEmail,
    string? CompanyPhone,
    string? CompanyAddress,
    string? DefaultCommercialTerms) : ICommand;
