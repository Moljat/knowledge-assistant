using System.Text;
using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.Application.Ai;

public interface IChatHandler
{
    Task<ChatResponse> HandleAsync(ChatRequest request, CancellationToken cancellationToken = default);
}

public sealed record ChatRequest(string Question);

public sealed record ChatResponse(string? Answer);

public sealed class ChatHandler(
    IAiAnalysisService aiService,
    IKnowledgeRecordRepository repository) : IChatHandler
{
    private const int MaxContextRecords = 20;

    public async Task<ChatResponse> HandleAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var recordsPaged = await repository.ListAsync(
            new KnowledgeRecordListFilters(null, null, null, null, null),
            1,
            MaxContextRecords,
            cancellationToken);

        var context = BuildContext(recordsPaged.Items);

        var content = string.IsNullOrWhiteSpace(context)
            ? request.Question
            : $"Contexto del sistema de conocimiento:\n\n{context}\n\nPregunta del usuario:\n{request.Question}";

        var result = await aiService.AnalyzeAsync(
            new AiAnalysisRequest(content, AiAnalysisType.Chat),
            cancellationToken);

        return new ChatResponse(result.Answer);
    }

    private static string BuildContext(IReadOnlyList<KnowledgeRecord> records)
    {
        if (records.Count == 0) return string.Empty;

        var sb = new StringBuilder();
        foreach (var record in records)
        {
            sb.AppendLine($"--- Registro: {record.Title} ---");
            sb.AppendLine($"Categoria: {record.Category ?? "Sin clasificar"}");
            sb.AppendLine($"Contenido: {Truncate(record.Content, 500)}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength] + "...";
}
