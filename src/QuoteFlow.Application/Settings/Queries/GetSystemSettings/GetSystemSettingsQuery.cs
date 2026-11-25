using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Settings.Common;

namespace QuoteFlow.Application.Settings.Queries.GetSystemSettings;

/// <summary>Consulta la configuración del sistema (RF-CFG-01). Solo Administrador.</summary>
public sealed record GetSystemSettingsQuery : IQuery<SystemSettingsDto>;
