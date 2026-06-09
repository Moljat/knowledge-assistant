using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Application.Records;
using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.UnitTests.Application;

public sealed class DeleteKnowledgeRecordHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRecordExists_RemovesAndSaves()
    {
        var record = KnowledgeRecord.Create(
            "Registro",
            "Contenido",
            null,
            KnowledgeRecordType.Note);
        var repository = new StubRepository(record);
        var unitOfWork = new CountingUnitOfWork();
        var handler = new DeleteKnowledgeRecordHandler(repository, unitOfWork);

        var deleted = await handler.HandleAsync(record.Id);

        Assert.True(deleted);
        Assert.Same(record, repository.RemovedRecord);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WhenRecordDoesNotExist_ReturnsFalseWithoutSaving()
    {
        var repository = new StubRepository(record: null);
        var unitOfWork = new CountingUnitOfWork();
        var handler = new DeleteKnowledgeRecordHandler(repository, unitOfWork);

        var deleted = await handler.HandleAsync(Guid.NewGuid());

        Assert.False(deleted);
        Assert.Null(repository.RemovedRecord);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private sealed class StubRepository(KnowledgeRecord? record) : IKnowledgeRecordRepository
    {
        public KnowledgeRecord? RemovedRecord { get; private set; }

        public Task<KnowledgeRecord?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(record?.Id == id ? record : null);
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
            return Task.FromResult(record?.Id == id);
        }

        public Task<PagedKnowledgeRecordResult> ListAsync(
            KnowledgeRecordListFilters filters,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var items = record is null
                ? []
                : new List<KnowledgeRecord> { record };

            return Task.FromResult(new PagedKnowledgeRecordResult(
                items,
                page,
                pageSize,
                items.Count));
        }

        public void Add(KnowledgeRecord record)
        {
        }

        public void Remove(KnowledgeRecord record)
        {
            RemovedRecord = record;
        }
    }

    private sealed class CountingUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }
}
