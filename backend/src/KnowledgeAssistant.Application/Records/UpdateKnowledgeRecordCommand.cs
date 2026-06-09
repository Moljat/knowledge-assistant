using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.Application.Records;

public sealed record UpdateKnowledgeRecordCommand(
    Guid Id,
    string Title,
    string Content,
    string? Source,
    KnowledgeRecordType Type);
