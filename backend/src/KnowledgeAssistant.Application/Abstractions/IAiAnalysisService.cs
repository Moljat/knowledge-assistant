using KnowledgeAssistant.Application.Ai;

namespace KnowledgeAssistant.Application.Abstractions;

public interface IAiAnalysisService
{
    Task<AiAnalysisResult> AnalyzeAsync(
        AiAnalysisRequest request,
        CancellationToken cancellationToken = default);
}
