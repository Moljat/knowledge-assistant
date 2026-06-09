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

    public async Task<PagedKnowledgeRecordResult> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalItems = await context.KnowledgeRecords
            .AsNoTracking()
            .CountAsync(cancellationToken);
        var items = await context.KnowledgeRecords
            .AsNoTracking()
            .OrderByDescending(record => record.CreatedAtUtc)
            .ThenBy(record => record.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedKnowledgeRecordResult(
            items,
            page,
            pageSize,
            totalItems);
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
