namespace KnowledgeAssistant.Domain.Entities;

public sealed class KnowledgeRecord
{
    public const int MaxTitleLength = 200;
    public const int MaxContentLength = 50_000;
    public const int MaxSourceLength = 500;
    public const int MaxCategoryLength = 100;
    public const int MaxSummaryLength = 4_000;
    public const int MaxRecommendationsLength = 8_000;
    public const int MaxAiErrorLength = 1_000;

    private KnowledgeRecord()
    {
    }

    private KnowledgeRecord(
        string title,
        string content,
        string? source,
        KnowledgeRecordType type,
        DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        Title = NormalizeRequired(title, nameof(title), MaxTitleLength);
        Content = NormalizeRequired(content, nameof(content), MaxContentLength);
        Source = NormalizeOptional(source, nameof(source), MaxSourceLength);
        Type = EnsureDefined(type, nameof(type));
        Status = KnowledgeRecordStatus.Draft;
        AiStatus = AiProcessingStatus.NotRequested;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = null!;

    public string Content { get; private set; } = null!;

    public string? Source { get; private set; }

    public KnowledgeRecordType Type { get; private set; }

    public KnowledgeRecordStatus Status { get; private set; }

    public AiProcessingStatus AiStatus { get; private set; }

    public string? Category { get; private set; }

    public string? Summary { get; private set; }

    public string? Recommendations { get; private set; }

    public string? AiError { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public DateTimeOffset? AiProcessedAtUtc { get; private set; }

    public static KnowledgeRecord Create(
        string title,
        string content,
        string? source,
        KnowledgeRecordType type = KnowledgeRecordType.Note,
        DateTimeOffset? now = null)
    {
        return new KnowledgeRecord(
            title,
            content,
            source,
            type,
            now ?? DateTimeOffset.UtcNow);
    }

    public void Update(
        string title,
        string content,
        string? source,
        KnowledgeRecordType type,
        DateTimeOffset? now = null)
    {
        EnsureNotArchived();

        var normalizedTitle = NormalizeRequired(title, nameof(title), MaxTitleLength);
        var normalizedContent = NormalizeRequired(content, nameof(content), MaxContentLength);
        var normalizedSource = NormalizeOptional(source, nameof(source), MaxSourceLength);
        var definedType = EnsureDefined(type, nameof(type));
        var contentChanged = !string.Equals(
            Content,
            normalizedContent,
            StringComparison.Ordinal);

        Title = normalizedTitle;
        Content = normalizedContent;
        Source = normalizedSource;
        Type = definedType;

        if (contentChanged)
        {
            ResetAiAnalysis();
        }

        UpdatedAtUtc = now ?? DateTimeOffset.UtcNow;
    }

    public void Activate(DateTimeOffset? now = null)
    {
        if (Status == KnowledgeRecordStatus.Active)
        {
            return;
        }

        if (Status == KnowledgeRecordStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived knowledge record must be restored before it can be activated.");
        }

        Status = KnowledgeRecordStatus.Active;
        UpdatedAtUtc = now ?? DateTimeOffset.UtcNow;
    }

    public void Archive(DateTimeOffset? now = null)
    {
        if (Status == KnowledgeRecordStatus.Archived)
        {
            return;
        }

        if (AiStatus is AiProcessingStatus.Pending or AiProcessingStatus.Processing)
        {
            throw new InvalidOperationException(
                "A knowledge record cannot be archived while AI analysis is in progress.");
        }

        var timestamp = now ?? DateTimeOffset.UtcNow;
        Status = KnowledgeRecordStatus.Archived;
        ArchivedAtUtc = timestamp;
        UpdatedAtUtc = timestamp;
    }

    public void Restore(DateTimeOffset? now = null)
    {
        if (Status != KnowledgeRecordStatus.Archived)
        {
            throw new InvalidOperationException(
                "Only archived knowledge records can be restored.");
        }

        Status = KnowledgeRecordStatus.Active;
        ArchivedAtUtc = null;
        UpdatedAtUtc = now ?? DateTimeOffset.UtcNow;
    }

    public void RequestAiAnalysis(DateTimeOffset? now = null)
    {
        EnsureNotArchived();

        if (AiStatus is AiProcessingStatus.Pending or AiProcessingStatus.Processing)
        {
            throw new InvalidOperationException("AI analysis is already pending or processing.");
        }

        ClearAiResults();
        AiStatus = AiProcessingStatus.Pending;
        UpdatedAtUtc = now ?? DateTimeOffset.UtcNow;
    }

    public void StartAiAnalysis(DateTimeOffset? now = null)
    {
        EnsureNotArchived();

        if (AiStatus != AiProcessingStatus.Pending)
        {
            throw new InvalidOperationException(
                "AI analysis can only start when it is pending.");
        }

        AiStatus = AiProcessingStatus.Processing;
        UpdatedAtUtc = now ?? DateTimeOffset.UtcNow;
    }

    public void CompleteAiAnalysis(
        string? summary,
        string? category,
        string? recommendations,
        DateTimeOffset? now = null)
    {
        EnsureNotArchived();

        if (AiStatus != AiProcessingStatus.Processing)
        {
            throw new InvalidOperationException(
                "AI analysis can only complete when it is processing.");
        }

        var normalizedSummary = NormalizeOptional(
            summary,
            nameof(summary),
            MaxSummaryLength);
        var normalizedCategory = NormalizeOptional(
            category,
            nameof(category),
            MaxCategoryLength);
        var normalizedRecommendations = NormalizeOptional(
            recommendations,
            nameof(recommendations),
            MaxRecommendationsLength);

        if (normalizedSummary is null
            && normalizedCategory is null
            && normalizedRecommendations is null)
        {
            throw new ArgumentException(
                "At least one AI analysis result is required.");
        }

        var timestamp = now ?? DateTimeOffset.UtcNow;
        Summary = normalizedSummary;
        Category = normalizedCategory;
        Recommendations = normalizedRecommendations;
        AiError = null;
        AiStatus = AiProcessingStatus.Completed;
        AiProcessedAtUtc = timestamp;
        UpdatedAtUtc = timestamp;
    }

    public void FailAiAnalysis(string error, DateTimeOffset? now = null)
    {
        EnsureNotArchived();

        if (AiStatus is not (AiProcessingStatus.Pending or AiProcessingStatus.Processing))
        {
            throw new InvalidOperationException(
                "AI analysis can only fail when it is pending or processing.");
        }

        var timestamp = now ?? DateTimeOffset.UtcNow;
        AiError = NormalizeRequired(error, nameof(error), MaxAiErrorLength);
        AiStatus = AiProcessingStatus.Failed;
        AiProcessedAtUtc = timestamp;
        UpdatedAtUtc = timestamp;
    }

    private void ResetAiAnalysis()
    {
        ClearAiResults();
        AiStatus = AiProcessingStatus.NotRequested;
    }

    private void ClearAiResults()
    {
        Summary = null;
        Category = null;
        Recommendations = null;
        AiError = null;
        AiProcessedAtUtc = null;
    }

    private void EnsureNotArchived()
    {
        if (Status == KnowledgeRecordStatus.Archived)
        {
            throw new InvalidOperationException(
                "Archived knowledge records cannot be modified or analyzed.");
        }
    }

    private static string NormalizeRequired(
        string value,
        string parameterName,
        int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        var normalized = value.Trim();
        EnsureMaxLength(normalized, parameterName, maxLength);
        return normalized;
    }

    private static string? NormalizeOptional(
        string? value,
        string parameterName,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        EnsureMaxLength(normalized, parameterName, maxLength);
        return normalized;
    }

    private static TEnum EnsureDefined<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                $"Unsupported {typeof(TEnum).Name} value.");
        }

        return value;
    }

    private static void EnsureMaxLength(
        string value,
        string parameterName,
        int maxLength)
    {
        if (value.Length > maxLength)
        {
            throw new ArgumentException(
                $"{parameterName} cannot exceed {maxLength} characters.",
                parameterName);
        }
    }
}
