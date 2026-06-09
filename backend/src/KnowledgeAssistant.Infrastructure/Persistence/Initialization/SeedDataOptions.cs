namespace KnowledgeAssistant.Infrastructure.Persistence.Initialization;

public sealed class SeedDataOptions
{
    public const string SectionName = "SeedData";

    public bool Enabled { get; init; }
}
