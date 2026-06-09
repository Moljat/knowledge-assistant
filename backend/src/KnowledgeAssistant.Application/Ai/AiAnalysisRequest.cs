namespace KnowledgeAssistant.Application.Ai;

public sealed record AiAnalysisRequest(
    string Content,
    AiAnalysisType Type,
    string? Question = null);
