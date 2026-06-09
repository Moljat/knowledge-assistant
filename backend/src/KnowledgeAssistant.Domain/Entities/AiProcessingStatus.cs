namespace KnowledgeAssistant.Domain.Entities;

public enum AiProcessingStatus
{
    NotRequested = 0,
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}
