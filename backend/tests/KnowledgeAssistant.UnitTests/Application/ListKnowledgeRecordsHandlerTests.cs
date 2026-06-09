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
            new ListKnowledgeRecordsQuery(Page: 2, PageSize: 2));

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(7, result.TotalItems);
        Assert.Equal(4, result.TotalPages);
        Assert.Collection(
            result.Items,
            item => Assert.Equal("Uno", item.Title),
            item => Assert.Equal("Dos", item.Title));
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
            handler.HandleAsync(new ListKnowledgeRecordsQuery(page, pageSize)));

        Assert.Equal(parameterName, exception.ParamName);
    }

    private sealed class StubRepository(
        IReadOnlyList<KnowledgeRecord> records,
        int totalItems) : IKnowledgeRecordRepository
    {
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
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
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
