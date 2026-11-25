namespace QuoteFlow.Application.Quotes.Common;

/// <summary>
/// Acciones válidas según el estado de la cotización y los permisos del usuario (RF-COT-13).
/// </summary>
public sealed record QuoteActionsDto(
    bool CanEdit,
    bool CanIssue,
    bool CanAccept,
    bool CanReject,
    bool CanCancel,
    bool CanDuplicate,
    bool CanDownloadPdf,
    bool CanUseAssistant);
