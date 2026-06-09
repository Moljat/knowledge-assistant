namespace KnowledgeAssistant.Infrastructure.Ai;

public sealed class MistralOptions
{
    public const string SectionName = "Mistral";

    public string BaseUrl { get; init; } = "https://api.mistral.ai/v1";

    public string Model { get; init; } = "mistral-small-latest";

    public string ApiKey { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 30;
}
