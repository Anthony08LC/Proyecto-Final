using QuoteFlow.Application.Abstractions.Messaging;
using QuoteFlow.Application.Abstractions.Persistence;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Common.Exceptions;
using QuoteFlow.Application.Common.Models;
using QuoteFlow.Application.Common.Validation;
using QuoteFlow.Application.Quotes.Common;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.Enums;
using QuoteFlow.Domain.Exceptions;

namespace QuoteFlow.Application.Assistant.Commands.SuggestQuoteText;

/// <summary>
/// Flujo de la sección 11.3: valida permisos, estado del borrador y límite de uso; arma el contexto
/// mínimo con el desglose del motor; llama al asistente y registra el evento de auditoría sin el
/// contenido. El borrador no cambia: la sugerencia se devuelve y el usuario decide si la acepta.
/// </summary>
internal sealed class SuggestQuoteTextCommandHandler(
    IQuoteRepository quotes,
    AssistantContextBuilder contextBuilder,
    AssistantUsageLimiter usageLimiter,
    ITextAssistant assistant,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IAuditLog auditLog)
    : ICommandHandler<SuggestQuoteTextCommand, QuoteTextSuggestionDto>
{
    public async Task<QuoteTextSuggestionDto> HandleAsync(
        SuggestQuoteTextCommand command,
        CancellationToken cancellationToken = default)
    {
        var (textType, action) = Validate(command);

        if (!assistant.IsAvailable)
        {
            throw AssistantUnavailableException.NotConfigured();
        }

        var quote = await quotes.GetRequiredAsync(command.QuoteId, cancellationToken);
        currentUser.EnsureCanManage(quote);

        if (quote.Status != QuoteStatus.Draft)
        {
            throw new DomainException("El asistente de redacción solo se puede usar en cotizaciones en Borrador.");
        }

        await usageLimiter.EnsureWithinLimitAsync(currentUser.UserId, cancellationToken);

        var original = action == AssistantAction.Improve
            ? TextToImprove(command, quote, textType)
            : null;

        var context = await contextBuilder.BuildAsync(quote, textType, cancellationToken);

        // El contexto recalculó el borrador en memoria: nada de eso se guarda (sección 11.2).
        unitOfWork.DiscardChanges();

        var prompt = AssistantPrompts.Build(
            textType,
            action,
            context,
            original is null ? null : SensitiveDataFilter.Redact(original));

        var raw = await assistant.CompleteAsync(prompt, cancellationToken);
        var suggestion = AssistantOutput.Normalize(raw) ?? throw AssistantUnavailableException.ServiceError();

        auditLog.Record(new AuditEntry(
            QuoteAuditEvents.TextGenerated,
            AuditEntityType.Quote,
            quote.Id,
            $"Sugerencia de IA para «{AssistantPrompts.Label(textType)}» (acción: {AssistantPrompts.Label(action)})."));

        // Se registra aunque el cliente se haya desconectado: la solicitud ya consumió el servicio.
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var remaining = await usageLimiter.GetRemainingAsync(currentUser.UserId, CancellationToken.None);

        return new QuoteTextSuggestionDto(textType, action, suggestion, original, remaining);
    }

    private static (AssistantTextType TextType, AssistantAction Action) Validate(SuggestQuoteTextCommand command)
    {
        var errors = new ValidationErrors();

        if (command.TextType is not { } textType || !Enum.IsDefined(textType))
        {
            errors.Add("textType", "Indique el texto: introducción, resumen ejecutivo, observaciones o condiciones comerciales.");
        }

        if (command.Action is not { } action || !Enum.IsDefined(action))
        {
            errors.Add("action", "Indique la acción: generar o mejorar.");
        }

        if (command.CurrentText is { Length: > Quote.TextMaxLength })
        {
            errors.Add("currentText", $"El texto no puede superar los {Quote.TextMaxLength} caracteres.");
        }

        errors.ThrowIfAny();
        return (command.TextType!.Value, command.Action!.Value);
    }

    private static string TextToImprove(SuggestQuoteTextCommand command, Quote quote, AssistantTextType textType)
    {
        var text = (command.CurrentText ?? SavedText(quote, textType))?.Trim();

        if (string.IsNullOrEmpty(text))
        {
            var errors = new ValidationErrors();
            errors.Add("currentText", "No hay texto para mejorar. Escriba un texto o use «Generar».");
            errors.ThrowIfAny();
        }

        return text!;
    }

    private static string? SavedText(Quote quote, AssistantTextType textType) => textType switch
    {
        AssistantTextType.Introduction => quote.Introduction,
        AssistantTextType.ExecutiveSummary => quote.ExecutiveSummary,
        AssistantTextType.Notes => quote.Notes,
        AssistantTextType.CommercialTerms => quote.CommercialTerms,
        _ => null
    };
}
