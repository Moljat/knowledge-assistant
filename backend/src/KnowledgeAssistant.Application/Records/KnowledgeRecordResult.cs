using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.Application.Records;

public sealed record KnowledgeRecordResult(
    Guid Id,
    string Title,
    string Content,
    string? Source,
    KnowledgeRecordType Type,
    KnowledgeRecordStatus Status,
    AiProcessingStatus AiStatus,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc)
{
    public static KnowledgeRecordResult FromEntity(KnowledgeRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return new KnowledgeRecordResult(
            record.Id,
            record.Title,
            record.Content,
            record.Source,
            record.Type,
            record.Status,
            record.AiStatus,
            record.CreatedAtUtc,
            record.UpdatedAtUtc);
    }
}
