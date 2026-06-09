using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.Application.Abstractions;

public sealed record DashboardStats(
    int TotalRecords,
    IReadOnlyDictionary<string, int> ByStatus,
    IReadOnlyDictionary<string, int> ByType,
    IReadOnlyDictionary<string, int> ByAiStatus,
    int TotalRetries)
{
    public static DashboardStats Empty { get; } = new(
        0,
        MapDefaults<KnowledgeRecordStatus>(),
        MapDefaults<KnowledgeRecordType>(),
        MapDefaults<AiProcessingStatus>(),
        0);

    public static DashboardStats Create(
        int totalRecords,
        IReadOnlyDictionary<KnowledgeRecordStatus, int> byStatus,
        IReadOnlyDictionary<KnowledgeRecordType, int> byType,
        IReadOnlyDictionary<AiProcessingStatus, int> byAiStatus,
        int totalRetries = 0)
    {
        return new DashboardStats(
            totalRecords,
            byStatus.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
            byType.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
            byAiStatus.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
            totalRetries);
    }

    private static Dictionary<string, int> MapDefaults<TEnum>()
        where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .ToDictionary(e => e.ToString(), _ => 0);
}
