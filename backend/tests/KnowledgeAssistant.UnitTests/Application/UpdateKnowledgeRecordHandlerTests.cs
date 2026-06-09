using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Application.Records;
using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.UnitTests.Application;

public sealed class UpdateKnowledgeRecordHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithExistingRecord_UpdatesAndSaves()
    {
        var record = KnowledgeRecord.Create(
            "Titulo original",
            "Contenido original",
            "Intranet",
            KnowledgeRecordType.Note);
        var repository = new StubRepository(record);
        var unitOfWork = new CountingUnitOfWork();
        var handler = new UpdateKnowledgeRecordHandler(repository, unitOfWork);

        var result = await handler.HandleAsync(new UpdateKnowledgeRecordCommand(
            record.Id,
            "  Titulo actualizado  ",
            "  Contenido actualizado  ",
            "  ERP  ",
            KnowledgeRecordType.BusinessRecord));

        Assert.NotNull(result);
        Assert.Equal("Titulo actualizado", result.Title);
        Assert.Equal("Contenido actualizado", result.Content);
        Assert.Equal("ERP", result.Source);
        Assert.Equal(KnowledgeRecordType.BusinessRecord, result.Type);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WhenRecordDoesNotExist_ReturnsNullWithoutSaving()
    {
        var unitOfWork = new CountingUnitOfWork();
        var handler = new UpdateKnowledgeRecordHandler(
            new StubRepository(record: null),
            unitOfWork);

        var result = await handler.HandleAsync(new UpdateKnowledgeRecordCommand(
            Guid.NewGuid(),
            "Titulo",
            "Contenido",
            null,
            KnowledgeRecordType.Note));

        Assert.Null(result);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidCommand_DoesNotSave()
    {
        var record = KnowledgeRecord.Create(
            "Titulo original",
            "Contenido original",
            null,
            KnowledgeRecordType.Note);
        var unitOfWork = new CountingUnitOfWork();
        var handler = new UpdateKnowledgeRecordHandler(
            new StubRepository(record),
            unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new UpdateKnowledgeRecordCommand(
                record.Id,
                " ",
                "Contenido",
                null,
                KnowledgeRecordType.Note)));

        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task HandleAsync_WhenContentChanges_ResetsCompletedAiAnalysis()
    {
        var record = KnowledgeRecord.Create(
            "Titulo",
            "Contenido original",
            null,
            KnowledgeRecordType.Note);
        record.RequestAiAnalysis();
        record.StartAiAnalysis();
        record.CompleteAiAnalysis("Resumen", "Categoria", "Recomendacion");
        var handler = new UpdateKnowledgeRecordHandler(
            new StubRepository(record),
            new CountingUnitOfWork());

        var result = await handler.HandleAsync(new UpdateKnowledgeRecordCommand(
            record.Id,
            record.Title,
            "Contenido nuevo",
            record.Source,
            record.Type));

        Assert.NotNull(result);
        Assert.Equal(AiProcessingStatus.NotRequested, result.AiStatus);
    }

    private sealed class StubRepository(KnowledgeRecord? record) : IKnowledgeRecordRepository
    {
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
