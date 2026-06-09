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

        return ParseResult(content, request.Type);
    }

    private static string BuildSystemPrompt(AiAnalysisType type) => type switch
    {
        AiAnalysisType.Summary =>
            "You are a business analyst. Summarize the following content in Spanish. "
            + "Respond with JSON: {\"summary\": \"...\"}. Max 4000 characters.",

        AiAnalysisType.Classification =>
            "You are a document classifier. Classify the content into exactly one category "
            + "from: Estrategia, Operaciones, Finanzas, Recursos Humanos, Legal, Tecnologia, Mercadeo, Ventas, Clientes, Proveedores. "
            + "Respond with JSON: {\"category\": \"...\"}. Max 100 characters.",

        AiAnalysisType.Recommendations =>
            "You are a business consultant. Based on the content, suggest actionable recommendations in Spanish. "
            + "Respond with JSON: {\"recommendations\": [\"...\", \"...\"]}. Max 5 recommendations, 8000 characters total.",

        AiAnalysisType.Question =>
            "You are a knowledge assistant. Answer the question based strictly on the provided context. "
            + "If the context does not contain the answer, say so. Respond in Spanish. "
            + "Respond with JSON: {\"answer\": \"...\"}. Max 2000 characters.",

        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

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
