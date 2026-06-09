using KnowledgeAssistant.Application;
using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Application.Ai;
using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.UnitTests.Application;

public sealed class CoverageRiskTests
{
    [Fact]
    public async Task ChatHandler_WithoutRecords_SendsQuestionWithoutContext()
    {
        var aiService = new CapturingAiService("Respuesta");
        var repository = new StubRepository([]);
        var handler = new ChatHandler(aiService, repository);

        var response = await handler.HandleAsync(new ChatRequest("¿Qué sabes?"));

        Assert.Equal("Respuesta", response.Answer);
        Assert.Equal("¿Qué sabes?", aiService.LastRequest?.Content);
        Assert.Equal(AiAnalysisType.Chat, aiService.LastRequest?.Type);
        Assert.Equal(1, repository.LastPage);
        Assert.Equal(20, repository.LastPageSize);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ChatHandler_WithBlankQuestion_RejectsBeforeCallingDependencies(
        string question)
    {
        var aiService = new CapturingAiService("Respuesta");
        var repository = new StubRepository([]);
        var handler = new ChatHandler(aiService, repository);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new ChatRequest(question)));

        Assert.Null(aiService.LastRequest);
        Assert.Equal(0, repository.LastPage);
    }

    [Fact]
    public async Task ChatHandler_WithOversizedQuestion_RejectsInput()
    {
        var handler = new ChatHandler(
            new CapturingAiService("Respuesta"),
            new StubRepository([]));

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new ChatRequest(
                new string('x', AiInputValidator.MaxQuestionLength + 1))));

        Assert.Equal("Question", exception.ParamName);
    }

    [Fact]
    public async Task ChatHandler_WithRecords_AddsBoundedContext()
    {
        var longContent = new string('x', 550);
        var classified = KnowledgeRecord.Create("Política", longContent, null);
        classified.RequestAiAnalysis();
        classified.StartAiAnalysis();
        classified.CompleteAiAnalysis("Resumen", "Operaciones", null);
        var unclassified = KnowledgeRecord.Create("Nota", "Contenido breve", null);
        var aiService = new CapturingAiService(null);
        var handler = new ChatHandler(
            aiService,
            new StubRepository([classified, unclassified]));

        await handler.HandleAsync(new ChatRequest("Resume el contexto"));

        var content = aiService.LastRequest?.Content;
        Assert.NotNull(content);
        Assert.Contains("--- Registro: Política ---", content);
        Assert.Contains("Categoria: Operaciones", content);
        Assert.Contains(new string('x', 500) + "...", content);
        Assert.Contains("Categoria: Sin clasificar", content);
        Assert.EndsWith("Pregunta del usuario:\nResume el contexto", content);
    }

    [Fact]
    public async Task DashboardHandler_ReturnsRepositoryStats()
    {
        var stats = DashboardStats.Create(
            4,
            new Dictionary<KnowledgeRecordStatus, int>
            {
                [KnowledgeRecordStatus.Active] = 3
            },
            new Dictionary<KnowledgeRecordType, int>
            {
                [KnowledgeRecordType.Document] = 2
            },
            new Dictionary<AiProcessingStatus, int>
            {
                [AiProcessingStatus.Completed] = 1
            },
            totalRetries: 2);
        var handler = new GetDashboardStatsHandler(new StubRepository([], stats));

        var result = await handler.HandleAsync();

        Assert.Same(stats, result);
        Assert.Equal(4, result.TotalRecords);
        Assert.Equal(3, result.ByStatus["Active"]);
        Assert.Equal(2, result.ByType["Document"]);
        Assert.Equal(1, result.ByAiStatus["Completed"]);
        Assert.Equal(2, result.TotalRetries);
    }

    [Fact]
    public void DashboardStats_Empty_ContainsEveryEnumValue()
    {
        Assert.All(
            Enum.GetNames<KnowledgeRecordStatus>(),
            name => Assert.Equal(0, DashboardStats.Empty.ByStatus[name]));
        Assert.All(
            Enum.GetNames<KnowledgeRecordType>(),
            name => Assert.Equal(0, DashboardStats.Empty.ByType[name]));
        Assert.All(
            Enum.GetNames<AiProcessingStatus>(),
            name => Assert.Equal(0, DashboardStats.Empty.ByAiStatus[name]));
    }

    private sealed class CapturingAiService(string? answer) : IAiAnalysisService
    {
        public AiAnalysisRequest? LastRequest { get; private set; }

        public Task<AiAnalysisResult> AnalyzeAsync(
            AiAnalysisRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(new AiAnalysisResult(null, null, [], answer));
        }
    }

    private sealed class StubRepository(
        IReadOnlyList<KnowledgeRecord> records,
        DashboardStats? stats = null) : IKnowledgeRecordRepository
    {
        public int LastPage { get; private set; }
        public int LastPageSize { get; private set; }

        public Task<PagedKnowledgeRecordResult> ListAsync(
            KnowledgeRecordListFilters filters,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            LastPage = page;
            LastPageSize = pageSize;
            return Task.FromResult(new PagedKnowledgeRecordResult(
                records,
                page,
                pageSize,
                records.Count));
        }

        public Task<DashboardStats> GetDashboardStatsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(stats ?? DashboardStats.Empty);

        public Task<KnowledgeRecord?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<KnowledgeRecord?>(null);

        public Task<KnowledgeRecord?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<KnowledgeRecord?>(null);

        public Task<bool> ExistsAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public void Add(KnowledgeRecord record)
        {
        }

        public void Remove(KnowledgeRecord record)
        {
        }
    }
}
