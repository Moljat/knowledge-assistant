using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Application.Records;
using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.UnitTests.Application;

public sealed class CreateKnowledgeRecordHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_PersistsRecordAndReturnsCreatedState()
    {
        var repository = new CapturingRepository();
        var unitOfWork = new CountingUnitOfWork();
        var handler = new CreateKnowledgeRecordHandler(repository, unitOfWork);
        var command = new CreateKnowledgeRecordCommand(
            "  Politica comercial  ",
            "  Contenido validado  ",
            "  Intranet  ",
            KnowledgeRecordType.Document);

        var result = await handler.HandleAsync(command);

        Assert.NotNull(repository.AddedRecord);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(repository.AddedRecord.Id, result.Id);
        Assert.Equal("Politica comercial", result.Title);
        Assert.Equal("Contenido validado", result.Content);
        Assert.Equal("Intranet", result.Source);
        Assert.Equal(KnowledgeRecordType.Document, result.Type);
        Assert.Equal(KnowledgeRecordStatus.Draft, result.Status);
        Assert.Equal(AiProcessingStatus.NotRequested, result.AiStatus);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCommand_DoesNotPersist()
    {
        var repository = new CapturingRepository();
        var unitOfWork = new CountingUnitOfWork();
        var handler = new CreateKnowledgeRecordHandler(repository, unitOfWork);
        var command = new CreateKnowledgeRecordCommand(
            " ",
            "Contenido",
            null,
            KnowledgeRecordType.Note);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(command));

        Assert.Null(repository.AddedRecord);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    private sealed class CapturingRepository : IKnowledgeRecordRepository
    {
        public KnowledgeRecord? AddedRecord { get; private set; }

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
            return Task.FromResult(new PagedKnowledgeRecordResult([], page, pageSize, 0));
        }

        public void Add(KnowledgeRecord record)
        {
            AddedRecord = record;
        }

        public void Remove(KnowledgeRecord record)
        {
        }

        public Task<DashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(DashboardStats.Empty);
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
