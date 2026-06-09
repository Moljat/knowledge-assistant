using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Application.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KnowledgeAssistant.Infrastructure.Ai;

public sealed class MistralAiAnalysisService : IAiAnalysisService
{
    private readonly HttpClient _http;
    private readonly MistralOptions _options;
    private readonly ILogger<MistralAiAnalysisService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public MistralAiAnalysisService(
        HttpClient http,
        IOptions<MistralOptions> options,
        ILogger<MistralAiAnalysisService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiAnalysisResult> AnalyzeAsync(
        AiAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = BuildSystemPrompt(request.Type);
        var userMessage = request.Type == AiAnalysisType.Question
            ? $"Context:\n{request.Content}\n\nQuestion:\n{request.Question}"
            : request.Content;

        var body = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage },
            },
            response_format = new { type = "json_object" },
            temperature = 0.1,
        };

        using var response = await _http.PostAsJsonAsync(
            "chat/completions", body, JsonOptions, cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<MistralChatResponse>(JsonOptions, cancellationToken);

        var content = json?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning("Mistral returned empty content for {Type} analysis", request.Type);
            return EmptyResult();
        }

        try
        {
            return ParseResult(content, request.Type);
        }
        catch (JsonException exception)
        {
            _logger.LogError(
                exception,
                "Failed to parse Mistral response for {Type} analysis. Content: {Content}",
                request.Type,
                Truncate(content, 200));

            return EmptyResult();
        }
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "...";

    private static string BuildSystemPrompt(AiAnalysisType type) =>
        AiPrompts.BuildSystemPrompt(type);

    private static AiAnalysisResult ParseResult(string content, AiAnalysisType type)
    {
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        return type switch
        {
            AiAnalysisType.Summary => new AiAnalysisResult(
                Summary: root.TryGetProperty("summary", out var s) ? s.GetString() : null,
                Category: null,
                Recommendations: [],
                Answer: null),

            AiAnalysisType.Classification => new AiAnalysisResult(
                Summary: null,
                Category: root.TryGetProperty("category", out var c) ? c.GetString() : null,
                Recommendations: [],
                Answer: null),

            AiAnalysisType.Recommendations => new AiAnalysisResult(
                Summary: null,
                Category: null,
                Recommendations: root.TryGetProperty("recommendations", out var r)
                    ? r.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                    : [],
                Answer: null),

            AiAnalysisType.Question => new AiAnalysisResult(
                Summary: null,
                Category: null,
                Recommendations: [],
                Answer: root.TryGetProperty("answer", out var a) ? a.GetString() : null),

            AiAnalysisType.Chat => new AiAnalysisResult(
                Summary: null,
                Category: null,
                Recommendations: [],
                Answer: root.TryGetProperty("answer", out var a) ? a.GetString() : null),

            _ => EmptyResult()
        };
    }

    private static AiAnalysisResult EmptyResult() => new(null, null, [], null);

    private sealed class MistralChatResponse
    {
        public Choice[]? Choices { get; init; }
    }

    private sealed class Choice
    {
        public Message? Message { get; init; }
    }

    private sealed class Message
    {
        public string? Content { get; init; }
    }
}
