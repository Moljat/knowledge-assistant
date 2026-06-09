using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.Application.Records;

public sealed record ListKnowledgeRecordsQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Category,
    KnowledgeRecordStatus? Status,
    KnowledgeRecordType? Type,
    AiProcessingStatus? AiStatus);
