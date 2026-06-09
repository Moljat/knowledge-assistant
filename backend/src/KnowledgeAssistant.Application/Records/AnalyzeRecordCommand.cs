using KnowledgeAssistant.Application.Ai;
using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.Application.Records;

public sealed record AnalyzeRecordCommand(
    Guid RecordId,
    AiAnalysisType Type,
    string? Question = null);
