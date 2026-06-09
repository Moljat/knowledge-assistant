using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.UnitTests.Domain;

public sealed class KnowledgeRecordTests
{
    private static readonly DateTimeOffset InitialTime =
        new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_NormalizesValuesAndSetsInitialState()
    {
        var record = KnowledgeRecord.Create(
            "  Política de ventas  ",
            "  Contenido empresarial  ",
            "  Intranet  ",
            KnowledgeRecordType.Document,
            InitialTime);

        Assert.NotEqual(Guid.Empty, record.Id);
        Assert.Equal("Política de ventas", record.Title);
        Assert.Equal("Contenido empresarial", record.Content);
        Assert.Equal("Intranet", record.Source);
        Assert.Equal(KnowledgeRecordType.Document, record.Type);
        Assert.Equal(KnowledgeRecordStatus.Draft, record.Status);
        Assert.Equal(AiProcessingStatus.NotRequested, record.AiStatus);
        Assert.Equal(InitialTime, record.CreatedAtUtc);
        Assert.Equal(InitialTime, record.UpdatedAtUtc);
        Assert.Null(record.ArchivedAtUtc);
        Assert.Null(record.AiProcessedAtUtc);
    }

    [Theory]
    [InlineData("", "Contenido")]
    [InlineData("Título", " ")]
    public void Create_WithoutRequiredContent_Throws(string title, string content)
    {
        Assert.Throws<ArgumentException>(() =>
            KnowledgeRecord.Create(title, content, null));
    }

    [Fact]
    public void Create_WithUnsupportedType_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            KnowledgeRecord.Create(
                "Título",
                "Contenido",
                null,
                (KnowledgeRecordType)999));
    }

    [Theory]
    [InlineData(KnowledgeRecord.MaxTitleLength + 1, "title")]
    [InlineData(KnowledgeRecord.MaxContentLength + 1, "content")]
    [InlineData(KnowledgeRecord.MaxSourceLength + 1, "source")]
    public void Create_WhenValueExceedsLimit_Throws(int length, string field)
    {
        var title = field == "title" ? new string('a', length) : "Título";
        var content = field == "content" ? new string('a', length) : "Contenido";
        var source = field == "source" ? new string('a', length) : null;

        Assert.Throws<ArgumentException>(() =>
            KnowledgeRecord.Create(title, content, source));
    }

    [Fact]
    public void Activate_FromDraft_SetsActiveStatus()
    {
        var record = CreateRecord();
        var activatedAt = InitialTime.AddMinutes(1);

        record.Activate(activatedAt);

        Assert.Equal(KnowledgeRecordStatus.Active, record.Status);
        Assert.Equal(activatedAt, record.UpdatedAtUtc);
    }

    [Fact]
    public void Archive_ThenRestore_ReturnsRecordToActive()
    {
        var record = CreateRecord();
        var archivedAt = InitialTime.AddMinutes(1);
        var restoredAt = InitialTime.AddMinutes(2);

        record.Archive(archivedAt);

        Assert.Equal(KnowledgeRecordStatus.Archived, record.Status);
        Assert.Equal(archivedAt, record.ArchivedAtUtc);

        record.Restore(restoredAt);

        Assert.Equal(KnowledgeRecordStatus.Active, record.Status);
        Assert.Null(record.ArchivedAtUtc);
        Assert.Equal(restoredAt, record.UpdatedAtUtc);
    }

    [Fact]
    public void Update_WhenArchived_Throws()
    {
        var record = CreateRecord();
        record.Archive();

        Assert.Throws<InvalidOperationException>(() =>
            record.Update(
                "Nuevo título",
                "Nuevo contenido",
                null,
                KnowledgeRecordType.Note));
    }

    [Fact]
    public void Update_WhenContentChanges_ResetsCompletedAiAnalysis()
    {
        var record = CreateRecordWithCompletedAnalysis();
        var updatedAt = InitialTime.AddMinutes(4);

        record.Update(
            "Título actualizado",
            "Contenido actualizado",
            "ERP",
            KnowledgeRecordType.BusinessRecord,
            updatedAt);

        Assert.Equal("Título actualizado", record.Title);
        Assert.Equal(KnowledgeRecordType.BusinessRecord, record.Type);
        Assert.Equal(AiProcessingStatus.NotRequested, record.AiStatus);
        Assert.Null(record.Summary);
        Assert.Null(record.Category);
        Assert.Null(record.Recommendations);
        Assert.Null(record.AiProcessedAtUtc);
        Assert.Equal(updatedAt, record.UpdatedAtUtc);
    }

    [Fact]
    public void Update_WhenContentIsUnchanged_PreservesCompletedAiAnalysis()
    {
        var record = CreateRecordWithCompletedAnalysis();

        record.Update(
            "Título actualizado",
            record.Content,
            "ERP",
            KnowledgeRecordType.BusinessRecord,
            InitialTime.AddMinutes(4));

        Assert.Equal(AiProcessingStatus.Completed, record.AiStatus);
        Assert.Equal("Resumen", record.Summary);
        Assert.Equal("Operaciones", record.Category);
    }

    [Fact]
    public void Update_WithUnsupportedType_DoesNotPartiallyModifyRecord()
    {
        var record = CreateRecord();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            record.Update(
                "Título modificado",
                "Contenido modificado",
                "ERP",
                (KnowledgeRecordType)999));

        Assert.Equal("Título", record.Title);
        Assert.Equal("Contenido", record.Content);
        Assert.Equal("Intranet", record.Source);
        Assert.Equal(KnowledgeRecordType.Note, record.Type);
    }

    [Fact]
    public void AiAnalysis_ValidFlow_StoresNormalizedResults()
    {
        var record = CreateRecord();
        var completedAt = InitialTime.AddMinutes(3);

        record.RequestAiAnalysis(InitialTime.AddMinutes(1));
        record.StartAiAnalysis(InitialTime.AddMinutes(2));
        record.CompleteAiAnalysis(
            "  Resumen  ",
            "  Operaciones  ",
            "  Revisar proceso  ",
            completedAt);

        Assert.Equal(AiProcessingStatus.Completed, record.AiStatus);
        Assert.Equal("Resumen", record.Summary);
        Assert.Equal("Operaciones", record.Category);
        Assert.Equal("Revisar proceso", record.Recommendations);
        Assert.Null(record.AiError);
        Assert.Equal(completedAt, record.AiProcessedAtUtc);
    }

    [Fact]
    public void StartAiAnalysis_WithoutPendingRequest_Throws()
    {
        var record = CreateRecord();

        Assert.Throws<InvalidOperationException>(() =>
            record.StartAiAnalysis());
    }

    [Fact]
    public void CompleteAiAnalysis_WithoutResults_Throws()
    {
        var record = CreateRecord();
        record.RequestAiAnalysis();
        record.StartAiAnalysis();

        Assert.Throws<ArgumentException>(() =>
            record.CompleteAiAnalysis(null, " ", null));
    }

    [Fact]
    public void FailAiAnalysis_FromProcessing_StoresError()
    {
        var record = CreateRecord();
        var failedAt = InitialTime.AddMinutes(3);
        record.RequestAiAnalysis();
        record.StartAiAnalysis();

        record.FailAiAnalysis("  Mistral timeout  ", failedAt);

        Assert.Equal(AiProcessingStatus.Failed, record.AiStatus);
        Assert.Equal("Mistral timeout", record.AiError);
        Assert.Equal(failedAt, record.AiProcessedAtUtc);
    }

    [Fact]
    public void RequestAiAnalysis_AfterCompletion_ClearsPreviousResults()
    {
        var record = CreateRecordWithCompletedAnalysis();

        record.RequestAiAnalysis(InitialTime.AddMinutes(4));

        Assert.Equal(AiProcessingStatus.Pending, record.AiStatus);
        Assert.Null(record.Summary);
        Assert.Null(record.Category);
        Assert.Null(record.Recommendations);
        Assert.Null(record.AiProcessedAtUtc);
    }

    [Fact]
    public void Archive_WhileAiAnalysisIsPending_Throws()
    {
        var record = CreateRecord();
        record.RequestAiAnalysis();

        Assert.Throws<InvalidOperationException>(() =>
            record.Archive());
    }

    [Fact]
    public void RequestAiAnalysis_WhenArchived_Throws()
    {
        var record = CreateRecord();
        record.Archive();

        Assert.Throws<InvalidOperationException>(() =>
            record.RequestAiAnalysis());
    }

    private static KnowledgeRecord CreateRecord()
    {
        return KnowledgeRecord.Create(
            "Título",
            "Contenido",
            "Intranet",
            KnowledgeRecordType.Note,
            InitialTime);
    }

    private static KnowledgeRecord CreateRecordWithCompletedAnalysis()
    {
        var record = CreateRecord();
        record.RequestAiAnalysis(InitialTime.AddMinutes(1));
        record.StartAiAnalysis(InitialTime.AddMinutes(2));
        record.CompleteAiAnalysis(
            "Resumen",
            "Operaciones",
            "Revisar proceso",
            InitialTime.AddMinutes(3));
        return record;
    }
}
