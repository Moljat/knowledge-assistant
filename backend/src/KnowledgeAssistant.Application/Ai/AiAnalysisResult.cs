namespace KnowledgeAssistant.Application.Ai;

public sealed record AiAnalysisResult(
    string? Summary,
    string? Category,
    IReadOnlyList<string> Recommendations,
    string? Answer);
