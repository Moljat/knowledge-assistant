using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeAssistant.Infrastructure.Persistence.Repositories;

public sealed class KnowledgeRecordRepository(KnowledgeAssistantDbContext context)
    : IKnowledgeRecordRepository
{
    public Task<KnowledgeRecord?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return context.KnowledgeRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(record => record.Id == id, cancellationToken);
    }

    public Task<KnowledgeRecord?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return context.KnowledgeRecords
            .SingleOrDefaultAsync(record => record.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return context.KnowledgeRecords
            .AsNoTracking()
            .AnyAsync(record => record.Id == id, cancellationToken);
    }

    public void Add(KnowledgeRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        context.KnowledgeRecords.Add(record);
    }

    public void Remove(KnowledgeRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        context.KnowledgeRecords.Remove(record);
    }
}
