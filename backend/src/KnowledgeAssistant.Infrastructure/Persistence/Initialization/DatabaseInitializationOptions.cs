namespace KnowledgeAssistant.Infrastructure.Persistence.Initialization;

public sealed class DatabaseInitializationOptions
{
    public const string SectionName = "DatabaseInitialization";

    public bool Enabled { get; init; }
}
