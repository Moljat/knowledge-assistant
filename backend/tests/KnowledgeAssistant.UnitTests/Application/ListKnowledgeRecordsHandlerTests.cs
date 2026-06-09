using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Application.Records;
using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.UnitTests.Application;

public sealed class ListKnowledgeRecordsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidPagination_ReturnsPagedRecords()
    {
        var records = new[]
        {
            KnowledgeRecord.Create("Uno", "Contenido", null),
            KnowledgeRecord.Create("Dos", "Contenido", null)
        };
        var handler = new ListKnowledgeRecordsHandler(
            new StubRepository(records, totalItems: 7));

        var result = await handler.HandleAsync(
            new ListKnowledgeRecordsQuery(
                Page: 2,
                PageSize: 2,
                Search: null,
                Category: null,
                Status: null,
                Type: null,
                AiStatus: null));

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(7, result.TotalItems);
        Assert.Equal(4, result.TotalPages);
        Assert.Collection(
            result.Items,
            item => Assert.Equal("Uno", item.Title),
            item => Assert.Equal("Dos", item.Title));
    }

    [Fact]
    public async Task HandleAsync_WithFilters_NormalizesAndPassesFiltersToRepository()
    {
        var repository = new StubRepository([], totalItems: 0);
        var handler = new ListKnowledgeRecordsHandler(repository);

        await handler.HandleAsync(new ListKnowledgeRecordsQuery(
            Page: 1,
            PageSize: 20,
            Search: "  ventas  ",
            Category: "  Operaciones  ",
            Status: KnowledgeRecordStatus.Active,
            Type: KnowledgeRecordType.Document,
            AiStatus: AiProcessingStatus.Completed));

        Assert.NotNull(repository.LastFilters);
        Assert.Equal("ventas", repository.LastFilters.Search);
        Assert.Equal("Operaciones", repository.LastFilters.Category);
        Assert.Equal(KnowledgeRecordStatus.Active, repository.LastFilters.Status);
        Assert.Equal(KnowledgeRecordType.Document, repository.LastFilters.Type);
        Assert.Equal(AiProcessingStatus.Completed, repository.LastFilters.AiStatus);
    }

    [Theory]
    [InlineData(0, 20, "page")]
    [InlineData(1, 0, "pageSize")]
    [InlineData(1, 101, "pageSize")]
    public async Task HandleAsync_WithInvalidPagination_Throws(
        int page,
        int pageSize,
        string parameterName)
    {
        var handler = new ListKnowledgeRecordsHandler(
            new StubRepository([], totalItems: 0));

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            handler.HandleAsync(new ListKnowledgeRecordsQuery(
                page,
                pageSize,
                Search: null,
                Category: null,
                Status: null,
                Type: null,
                AiStatus: null)));

        Assert.Equal(parameterName, exception.ParamName);
    }

    [Theory]
    [InlineData(ListKnowledgeRecordsHandler.MaxSearchLength + 1, "Search")]
    [InlineData(KnowledgeRecord.MaxCategoryLength + 1, "Category")]
    public async Task HandleAsync_WhenTextFilterExceedsLimit_Throws(
        int length,
        string field)
    {
        var handler = new ListKnowledgeRecordsHandler(
            new StubRepository([], totalItems: 0));
        var search = field == "Search" ? new string('a', length) : null;
        var category = field == "Category" ? new string('a', length) : null;

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new ListKnowledgeRecordsQuery(
                Page: 1,
                PageSize: 20,
                Search: search,
                Category: category,
                Status: null,
                Type: null,
                AiStatus: null)));

        Assert.Equal(field, exception.ParamName);
    }

    private sealed class StubRepository(
        IReadOnlyList<KnowledgeRecord> records,
        int totalItems) : IKnowledgeRecordRepository
    {
        public KnowledgeRecordListFilters? LastFilters { get; private set; }

        public Task<KnowledgeRecord?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<KnowledgeRecord?>(null);
        }

        public Task<KnowledgeRecord?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<KnowledgeRecord?>(null);
        }

        public Task<bool> ExistsAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<PagedKnowledgeRecordResult> ListAsync(
            KnowledgeRecordListFilters filters,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            LastFilters = filters;
            return Task.FromResult(new PagedKnowledgeRecordResult(
                records,
                page,
                pageSize,
                totalItems));
        }

        public void Add(KnowledgeRecord record)
        {
        }

        public void Remove(KnowledgeRecord record)
        {
        }
    }
}
