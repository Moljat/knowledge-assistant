namespace KnowledgeAssistant.Domain.Entities;

public sealed class KnowledgeRecord
{
    public const int MaxTitleLength = 200;
    public const int MaxSourceLength = 500;

    private KnowledgeRecord()
    {
    }

    private KnowledgeRecord(string title, string content, string? source, DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        Title = NormalizeRequired(title, nameof(title));
        Content = NormalizeRequired(content, nameof(content));
        Source = NormalizeOptional(source);
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = null!;

    public string Content { get; private set; } = null!;

    public string? Source { get; private set; }

    public string? Category { get; private set; }

    public string? Summary { get; private set; }

    public string? Recommendations { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static KnowledgeRecord Create(
        string title,
        string content,
        string? source,
        DateTimeOffset? now = null)
    {
        return new KnowledgeRecord(title, content, source, now ?? DateTimeOffset.UtcNow);
    }

    public void Update(
        string title,
        string content,
        string? source,
        DateTimeOffset? now = null)
    {
        Title = NormalizeRequired(title, nameof(title));
        Content = NormalizeRequired(content, nameof(content));
        Source = NormalizeOptional(source);
        UpdatedAtUtc = now ?? DateTimeOffset.UtcNow;
    }

    public void ApplyAiAnalysis(
        string? summary,
        string? category,
        string? recommendations,
        DateTimeOffset? now = null)
    {
        Summary = NormalizeOptional(summary);
        Category = NormalizeOptional(category);
        Recommendations = NormalizeOptional(recommendations);
        UpdatedAtUtc = now ?? DateTimeOffset.UtcNow;
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
