using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.Application.Abstractions;

public interface IKnowledgeRecordRepository
{
    Task<KnowledgeRecord?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<KnowledgeRecord?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PagedKnowledgeRecordResult> ListAsync(
        KnowledgeRecordListFilters filters,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    void Add(KnowledgeRecord record);

    void Remove(KnowledgeRecord record);
}

public sealed record PagedKnowledgeRecordResult(
    IReadOnlyList<KnowledgeRecord> Items,
    int Page,
    int PageSize,
    int TotalItems);

public sealed record KnowledgeRecordListFilters(
    string? Search,
    string? Category,
    KnowledgeRecordStatus? Status,
    KnowledgeRecordType? Type,
    AiProcessingStatus? AiStatus);
