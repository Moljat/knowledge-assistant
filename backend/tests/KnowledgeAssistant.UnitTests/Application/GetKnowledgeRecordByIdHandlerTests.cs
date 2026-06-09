using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Application.Records;
using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.UnitTests.Application;

public sealed class GetKnowledgeRecordByIdHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRecordExists_ReturnsRecord()
    {
        var record = KnowledgeRecord.Create(
            "Registro",
            "Contenido",
            "Intranet",
            KnowledgeRecordType.Note);
        var handler = new GetKnowledgeRecordByIdHandler(
            new StubRepository([record]));

        var result = await handler.HandleAsync(record.Id);

        Assert.NotNull(result);
        Assert.Equal(record.Id, result.Id);
        Assert.Equal(record.Title, result.Title);
    }

    [Fact]
    public async Task HandleAsync_WhenRecordDoesNotExist_ReturnsNull()
    {
        var handler = new GetKnowledgeRecordByIdHandler(
            new StubRepository([]));

        var result = await handler.HandleAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    private sealed class StubRepository(
        IReadOnlyList<KnowledgeRecord> records) : IKnowledgeRecordRepository
    {
        public Task<KnowledgeRecord?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(records.SingleOrDefault(record => record.Id == id));
        }

        public Task<KnowledgeRecord?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return GetByIdAsync(id, cancellationToken);
        }

        public Task<bool> ExistsAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(records.Any(record => record.Id == id));
        }

        public Task<PagedKnowledgeRecordResult> ListAsync(
            KnowledgeRecordListFilters filters,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedKnowledgeRecordResult(records, page, pageSize, records.Count));
        }

        public void Add(KnowledgeRecord record)
        {
        }

        public void Remove(KnowledgeRecord record)
        {
        }
    }
}
