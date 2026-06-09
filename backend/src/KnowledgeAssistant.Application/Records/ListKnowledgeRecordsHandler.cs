using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Domain.Entities;

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
    public const int MaxSearchLength = 200;

    public async Task<PagedResult<KnowledgeRecordResult>> HandleAsync(
        ListKnowledgeRecordsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidatePagination(query.Page, query.PageSize);
        var filters = NormalizeFilters(query);

        var records = await repository.ListAsync(
            filters,
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

    private static KnowledgeRecordListFilters NormalizeFilters(ListKnowledgeRecordsQuery query)
    {
        var search = NormalizeOptional(query.Search, nameof(query.Search), MaxSearchLength);
        var category = NormalizeOptional(
            query.Category,
            nameof(query.Category),
            KnowledgeRecord.MaxCategoryLength);
        var status = EnsureDefined(query.Status, nameof(query.Status));
        var type = EnsureDefined(query.Type, nameof(query.Type));
        var aiStatus = EnsureDefined(query.AiStatus, nameof(query.AiStatus));

        return new KnowledgeRecordListFilters(
            search,
            category,
            status,
            type,
            aiStatus);
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
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"{parameterName} cannot exceed {maxLength} characters.",
                parameterName);
        }

        return normalized;
    }

    private static TEnum? EnsureDefined<TEnum>(
        TEnum? value,
        string parameterName)
        where TEnum : struct, Enum
    {
        if (value is null)
        {
            return null;
        }

        if (!Enum.IsDefined(value.Value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                $"Unsupported {typeof(TEnum).Name} value.");
        }

        return value;
    }
}
