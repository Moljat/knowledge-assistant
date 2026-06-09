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
        KnowledgeRecordListFilters filters,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(
            context.KnowledgeRecords.AsNoTracking(),
            filters);

        var totalItems = await query
            .CountAsync(cancellationToken);
        var items = await query
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

    private static IQueryable<KnowledgeRecord> ApplyFilters(
        IQueryable<KnowledgeRecord> query,
        KnowledgeRecordListFilters filters)
    {
        if (filters.Search is not null)
        {
            query = query.Where(record =>
                record.Title.Contains(filters.Search)
                || record.Content.Contains(filters.Search)
                || (record.Source != null && record.Source.Contains(filters.Search))
                || (record.Category != null && record.Category.Contains(filters.Search)));
        }

        if (filters.Category is not null)
        {
            query = query.Where(record => record.Category == filters.Category);
        }

        if (filters.Status is not null)
        {
            query = query.Where(record => record.Status == filters.Status);
        }

        if (filters.Type is not null)
        {
            query = query.Where(record => record.Type == filters.Type);
        }

        if (filters.AiStatus is not null)
        {
            query = query.Where(record => record.AiStatus == filters.AiStatus);
        }

        return query;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var totalRecords = await context.KnowledgeRecords
            .CountAsync(cancellationToken);

        var statusCounts = await context.KnowledgeRecords
            .GroupBy(r => r.Status)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Key, g => g.Count, cancellationToken);

        var typeCounts = await context.KnowledgeRecords
            .GroupBy(r => r.Type)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Key, g => g.Count, cancellationToken);

        var aiStatusCounts = await context.KnowledgeRecords
            .GroupBy(r => r.AiStatus)
            .Select(g => new { Key = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Key, g => g.Count, cancellationToken);

        var totalRetries = await context.KnowledgeRecords
            .SumAsync(r => (int?)r.AiRetryCount, cancellationToken) ?? 0;

        return DashboardStats.Create(
            totalRecords,
            statusCounts,
            typeCounts,
            aiStatusCounts,
            totalRetries);
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
