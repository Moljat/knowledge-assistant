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

    void Add(KnowledgeRecord record);

    void Remove(KnowledgeRecord record);
}
