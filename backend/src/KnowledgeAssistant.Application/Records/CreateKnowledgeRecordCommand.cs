using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.Application.Records;

public sealed record CreateKnowledgeRecordCommand(
    string Title,
    string Content,
    string? Source,
    KnowledgeRecordType Type);
