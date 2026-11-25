using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuoteFlow.Application.Abstractions.Services;
using QuoteFlow.Application.Assistant;
using QuoteFlow.Application.Common.Exceptions;

namespace QuoteFlow.Infrastructure.Assistant;

/// <summary>
/// Cliente de la API <c>generateContent</c> de Google Gemini (T-076). Cada llamada tiene un tiempo
/// límite de 20 segundos (RNF-REN-04). Los errores, las respuestas bloqueadas o vacías y el tiempo
/// excedido se traducen a <see cref="AssistantUnavailableException"/>. El log registra el modelo,
/// la duración y el resultado, nunca la clave, el contexto ni el texto generado (RNF-MAN-06).
/// </summary>
internal sealed partial class GeminiTextAssistant(
    HttpClient httpClient,
    IOptions<GeminiOptions> options,
    ILogger<GeminiTextAssistant> logger) : ITextAssistant
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly GeminiOptions _options = options.Value;

    public bool IsAvailable => _options.IsConfigured;

    public async Task<string> CompleteAsync(AssistantPrompt prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prompt);

        if (!_options.IsConfigured)
        {
            throw AssistantUnavailableException.NotConfigured();
        }

        var model = _options.ResolvedModel;
        if (!ModelName().IsMatch(model))
        {
            logger.LogError("El modelo de Gemini configurado no tiene un nombre válido.");
            throw AssistantUnavailableException.ServiceError();
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.Timeout);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"models/{Uri.EscapeDataString(model)}:generateContent")
            {
                Content = JsonContent.Create(BuildRequest(prompt), options: JsonOptions)
            };
            request.Headers.Add("x-goog-api-key", _options.ApiKey);

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Gemini respondió con error {StatusCode} (modelo {Model}, {ElapsedMs} ms).",
                    (int)response.StatusCode, model, stopwatch.ElapsedMilliseconds);
                throw AssistantUnavailableException.ServiceError();
            }

            var body = await response.Content.ReadFromJsonAsync<GenerateContentResponse>(JsonOptions, timeout.Token);
            var text = ExtractText(body, out var outcome);

            if (text is null)
            {
                logger.LogWarning(
                    "Gemini no devolvió un texto utilizable: {Outcome} (modelo {Model}, {ElapsedMs} ms).",
                    outcome, model, stopwatch.ElapsedMilliseconds);
                throw AssistantUnavailableException.ServiceError();
            }

            logger.LogInformation(
                "Sugerencia de Gemini generada (modelo {Model}, {ElapsedMs} ms, {Characters} caracteres).",
                model, stopwatch.ElapsedMilliseconds, text.Length);
            return text;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                "Gemini excedió el tiempo límite de {TimeoutSeconds} s (modelo {Model}).",
                _options.Timeout.TotalSeconds, model);
            throw AssistantUnavailableException.Timeout(ex);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            logger.LogWarning(
                "No se pudo completar la llamada a Gemini: {ErrorType} (modelo {Model}, {ElapsedMs} ms).",
                ex.GetType().Name, model, stopwatch.ElapsedMilliseconds);
            throw AssistantUnavailableException.ServiceError(ex);
        }
    }

    private GenerateContentRequest BuildRequest(AssistantPrompt prompt) => new(
        SystemInstruction: new Content(null, [new Part(prompt.SystemInstruction)]),
        Contents: [new Content("user", [new Part(prompt.UserMessage)])],
        GenerationConfig: new GenerationConfig(
            Temperature: _options.Temperature,
            MaxOutputTokens: _options.MaxOutputTokens,
            ResponseMimeType: "text/plain"));

    /// <summary>Une las partes de texto del primer candidato; nulo si la respuesta fue bloqueada o vacía.</summary>
    private static string? ExtractText(GenerateContentResponse? body, out string outcome)
    {
        if (body?.PromptFeedback?.BlockReason is { } blockReason)
        {
            outcome = $"solicitud bloqueada ({blockReason})";
            return null;
        }

        var candidate = body?.Candidates?.FirstOrDefault();
        if (candidate is null)
        {
            outcome = "sin candidatos";
            return null;
        }

        var text = string.Concat(candidate.Content?.Parts?
            .Where(p => p.Thought != true)
            .Select(p => p.Text) ?? []);

        if (string.IsNullOrWhiteSpace(text))
        {
            outcome = $"respuesta vacía ({candidate.FinishReason ?? "sin motivo"})";
            return null;
        }

        outcome = candidate.FinishReason ?? "STOP";
        return text;
    }

    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9.\-_]{0,99}$")]
    private static partial Regex ModelName();

    // Contrato de la API generateContent (solo los campos que se usan).
    private sealed record GenerateContentRequest(
        Content SystemInstruction,
        IReadOnlyList<Content> Contents,
        GenerationConfig GenerationConfig);

    private sealed record Content(string? Role, IReadOnlyList<Part>? Parts);

    private sealed record Part(string? Text, bool? Thought = null);

    private sealed record GenerationConfig(double Temperature, int MaxOutputTokens, string ResponseMimeType);

    private sealed record GenerateContentResponse(
        IReadOnlyList<Candidate>? Candidates,
        PromptFeedback? PromptFeedback);

    private sealed record Candidate(Content? Content, string? FinishReason);

    private sealed record PromptFeedback(string? BlockReason);
}
