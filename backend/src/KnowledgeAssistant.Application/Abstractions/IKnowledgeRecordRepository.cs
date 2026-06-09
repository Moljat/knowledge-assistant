using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.Application.Abstractions;

public interface IKnowledgeRecordRepository
{
    Task<KnowledgeRecord?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    void Add(KnowledgeRecord record);

    void Remove(KnowledgeRecord record);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
