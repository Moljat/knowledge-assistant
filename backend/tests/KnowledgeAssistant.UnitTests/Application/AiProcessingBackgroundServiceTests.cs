using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Application.Ai;
using KnowledgeAssistant.Domain.Entities;
using KnowledgeAssistant.Infrastructure.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace KnowledgeAssistant.UnitTests.Application;

public sealed class AiProcessingBackgroundServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WithPendingRecord_ProcessesAndCompletes()
    {
        var record = CreatePendingRecord();
        var repository = new StubRepository([record]);
        var aiService = new StubAiAnalysisService(new AiAnalysisResult("Summary", "Category", ["Rec1"], null));
        var unitOfWork = new CountingUnitOfWork();
        var sut = CreateService(repository, aiService, unitOfWork);

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(50);
        await sut.StopAsync(CancellationToken.None);

        Assert.Equal(AiProcessingStatus.Completed, record.AiStatus);
        Assert.Equal("Summary", record.Summary);
        Assert.Equal("Category", record.Category);
        Assert.Equal("Rec1", record.Recommendations);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAiServiceThrows_RecordsFailureAndRetries()
    {
        var record = CreatePendingRecord();
        var repository = new StubRepository([record]);
        var aiService = new StubAiAnalysisService(new InvalidOperationException("AI unavailable"));
        var unitOfWork = new CountingUnitOfWork();
        var sut = CreateService(repository, aiService, unitOfWork);

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(50);
        await sut.StopAsync(CancellationToken.None);

        Assert.Equal(AiProcessingStatus.Pending, record.AiStatus);
        Assert.Equal(1, record.AiRetryCount);
        Assert.Equal("AI unavailable", record.AiError);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAiServiceThrowsAndRetriesExhausted_MarksAsFailed()
    {
        var record = CreatePendingRecord();
        for (var i = 0; i < KnowledgeRecord.MaxAiRetryCount - 1; i++)
        {
            record.FailAiAnalysis("previous error " + i);
        }

        var repository = new StubRepository([record]);
        var aiService = new StubAiAnalysisService(new InvalidOperationException("Final failure"));
        var unitOfWork = new CountingUnitOfWork();
        var sut = CreateService(repository, aiService, unitOfWork);

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(50);
        await sut.StopAsync(CancellationToken.None);

        Assert.Equal(AiProcessingStatus.Failed, record.AiStatus);
        Assert.Equal(KnowledgeRecord.MaxAiRetryCount, record.AiRetryCount);
        Assert.Equal("Final failure", record.AiError);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRecordDisappears_SkipsSilently()
    {
        var record = CreatePendingRecord();
        var repository = new StubRepository([record]) { ReturnNullForUpdate = true };
        var aiService = new StubAiAnalysisService(new AiAnalysisResult("S", "C", [], null));
        var unitOfWork = new CountingUnitOfWork();
        var sut = CreateService(repository, aiService, unitOfWork);

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(50);
        await sut.StopAsync(CancellationToken.None);

        Assert.Equal(AiProcessingStatus.Pending, record.AiStatus);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultiplePendingRecords_ProcessesAll()
    {
        var record1 = CreatePendingRecord();
        var record2 = CreatePendingRecord();
        var record3 = CreatePendingRecord();
        var repository = new StubRepository([record1, record2, record3]);
        var aiService = new StubAiAnalysisService(new AiAnalysisResult("Summary", "Category", ["Rec"], null));
        var unitOfWork = new CountingUnitOfWork();
        var sut = CreateService(repository, aiService, unitOfWork);

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(50);
        await sut.StopAsync(CancellationToken.None);

        Assert.Equal(AiProcessingStatus.Completed, record1.AiStatus);
        Assert.Equal(AiProcessingStatus.Completed, record2.AiStatus);
        Assert.Equal(AiProcessingStatus.Completed, record3.AiStatus);
        Assert.Equal(3, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ExecuteAsync_RecoversFromExceptionInCycle()
    {
        var record = CreatePendingRecord();
        var repository = new StubRepository([record]);
        var failingAi = new StubAiAnalysisService(new InvalidOperationException("fail"));
        var unitOfWork = new CountingUnitOfWork();
        var sut = CreateService(repository, failingAi, unitOfWork);

        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(50);
        await sut.StopAsync(CancellationToken.None);

        Assert.NotNull(record.AiError);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ExecuteAsync_StopsGracefullyOnCancellation()
    {
        var record = CreatePendingRecord();
        var repository = new StubRepository([record]);
        var aiService = new StubAiAnalysisService(new AiAnalysisResult("S", "C", [], null));
        var unitOfWork = new CountingUnitOfWork();
        var sut = CreateService(repository, aiService, unitOfWork);

        var cts = new CancellationTokenSource();
        await sut.StartAsync(cts.Token);
        await Task.Delay(50);
        await sut.StopAsync(cts.Token);

        Assert.Equal(AiProcessingStatus.Completed, record.AiStatus);
    }

    private static KnowledgeRecord CreatePendingRecord()
    {
        var record = KnowledgeRecord.Create("Title", "Content", null, KnowledgeRecordType.Document);
        record.RequestAiAnalysis();
        return record;
    }

    private static AiProcessingBackgroundService CreateService(
        IKnowledgeRecordRepository repository,
        IAiAnalysisService aiService,
        IUnitOfWork unitOfWork)
    {
        var serviceProvider = new StubServiceProvider(repository, aiService, unitOfWork);
        return new AiProcessingBackgroundService(
            new StubServiceScopeFactory(serviceProvider),
            NullLogger<AiProcessingBackgroundService>.Instance);
    }

    private sealed class StubServiceScopeFactory(IServiceProvider serviceProvider) : IServiceScopeFactory
    {
        public IServiceScope CreateScope() => new StubServiceScope(serviceProvider);
    }

    private sealed class StubServiceScope(IServiceProvider serviceProvider) : IServiceScope
    {
        public IServiceProvider ServiceProvider => serviceProvider;
        public void Dispose() { }
    }

    private sealed class StubServiceProvider(
        IKnowledgeRecordRepository repository,
        IAiAnalysisService aiService,
        IUnitOfWork unitOfWork) : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IKnowledgeRecordRepository)) return repository;
            if (serviceType == typeof(IAiAnalysisService)) return aiService;
            if (serviceType == typeof(IUnitOfWork)) return unitOfWork;
            return null;
        }
    }

    private sealed class StubRepository(List<KnowledgeRecord> records) : IKnowledgeRecordRepository
    {
        public bool ReturnNullForUpdate { get; set; }

        public Task<PagedKnowledgeRecordResult> ListAsync(
            KnowledgeRecordListFilters filters,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var pending = records
                .Where(r => r.AiStatus == AiProcessingStatus.Pending)
                .ToList();
            return Task.FromResult(
                new PagedKnowledgeRecordResult(pending, page, pageSize, pending.Count));
        }

        public Task<KnowledgeRecord?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (ReturnNullForUpdate)
                return Task.FromResult<KnowledgeRecord?>(null);
            return Task.FromResult(records.FirstOrDefault(r => r.Id == id));
        }

        public Task<KnowledgeRecord?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(records.FirstOrDefault(r => r.Id == id));

        public Task<bool> ExistsAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(records.Any(r => r.Id == id));

        public Task<DashboardStats> GetDashboardStatsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(DashboardStats.Empty);

        public void Add(KnowledgeRecord record) => records.Add(record);

        public void Remove(KnowledgeRecord record) => records.Remove(record);
    }

    private sealed class StubAiAnalysisService : IAiAnalysisService
    {
        private readonly AiAnalysisResult? _result;
        private readonly Exception? _exception;

        public StubAiAnalysisService(AiAnalysisResult result)
        {
            _result = result;
        }

        public StubAiAnalysisService(Exception exception)
        {
            _exception = exception;
        }

        public Task<AiAnalysisResult> AnalyzeAsync(
            AiAnalysisRequest request,
            CancellationToken cancellationToken = default)
        {
            if (_exception is not null)
                throw _exception;
            return Task.FromResult(_result!);
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
