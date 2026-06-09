using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.Infrastructure.Persistence.Initialization;

public sealed record KnowledgeRecordSeedDefinition(
    string Title,
    string Content,
    KnowledgeRecordType Type,
    bool Activate);
