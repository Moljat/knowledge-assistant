using KnowledgeAssistant.Application.Abstractions;

namespace KnowledgeAssistant.Application.Records;

public interface IListKnowledgeRecordsHandler
{
    Task<PagedResult<KnowledgeRecordResult>> HandleAsync(
        ListKnowledgeRecordsQuery query,
        CancellationToken cancellationToken = default);
}

public sealed class ListKnowledgeRecordsHandler(
    IKnowledgeRecordRepository repository) : IListKnowledgeRecordsHandler
{
    public const int MinPage = 1;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;

    public async Task<PagedResult<KnowledgeRecordResult>> HandleAsync(
        ListKnowledgeRecordsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidatePagination(query.Page, query.PageSize);

        var records = await repository.ListAsync(
            query.Page,
            query.PageSize,
            cancellationToken);
        var items = records.Items
            .Select(KnowledgeRecordResult.FromEntity)
            .ToList();

        return new PagedResult<KnowledgeRecordResult>(
            items,
            records.Page,
            records.PageSize,
            records.TotalItems);
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < MinPage)
        {
            throw new ArgumentOutOfRangeException(
                nameof(page),
                page,
                $"page must be greater than or equal to {MinPage}.");
        }

        if (pageSize < MinPageSize || pageSize > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                $"pageSize must be between {MinPageSize} and {MaxPageSize}.");
        }
    }
}
