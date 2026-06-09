using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Domain.Entities;
using KnowledgeAssistant.Infrastructure;
using KnowledgeAssistant.Infrastructure.Persistence;
using KnowledgeAssistant.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeAssistant.IntegrationTests.Persistence;

[Collection(SqlServerCollection.Name)]
public sealed class KnowledgeRecordRepositoryTests(SqlServerFixture fixture)
{
    private static readonly DateTimeOffset InitialTime =
        new(2026, 6, 9, 12, 0, 0, TimeSpan.Zero);

    [SqlServerFact]
    public async Task AddAndGetById_PersistsRecordAndReturnsUntrackedEntity()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = fixture.ConnectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<KnowledgeAssistantDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IKnowledgeRecordRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var record = CreateRecord();

        repository.Add(record);
        var affectedRows = await unitOfWork.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var persisted = await repository.GetByIdAsync(record.Id);

        Assert.Equal(1, affectedRows);
        Assert.NotNull(persisted);
        Assert.Equal(record.Title, persisted.Title);
        Assert.Empty(context.ChangeTracker.Entries<KnowledgeRecord>());
    }

    [SqlServerFact]
    public async Task GetByIdForUpdate_TracksAndPersistsChanges()
    {
        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var repository = new KnowledgeRecordRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var record = CreateRecord();
        repository.Add(record);
        await unitOfWork.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var tracked = await repository.GetByIdForUpdateAsync(record.Id);
        tracked!.Update(
            "Registro actualizado",
            tracked.Content,
            tracked.Source,
            tracked.Type);
        await unitOfWork.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var persisted = await repository.GetByIdAsync(record.Id);

        Assert.Equal("Registro actualizado", persisted!.Title);
    }

    [SqlServerFact]
    public async Task Remove_DeletesPersistedRecord()
    {
        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var repository = new KnowledgeRecordRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var record = CreateRecord();
        repository.Add(record);
        await unitOfWork.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var tracked = await repository.GetByIdForUpdateAsync(record.Id);
        repository.Remove(tracked!);
        await unitOfWork.SaveChangesAsync();

        Assert.False(await repository.ExistsAsync(record.Id));
    }

    [SqlServerFact]
    public async Task AddAndGetById_PersistsLifecycleAndAiResultFields()
    {
        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var repository = new KnowledgeRecordRepository(context);
        var unitOfWork = new UnitOfWork(context);
        var record = KnowledgeRecord.Create(
            $"Analyzed record {Guid.NewGuid():N}",
            "Physical SQL Server lifecycle validation.",
            "Integration tests",
            KnowledgeRecordType.Document,
            InitialTime);
        var activatedAt = InitialTime.AddMinutes(1);
        var requestedAt = InitialTime.AddMinutes(2);
        var startedAt = InitialTime.AddMinutes(3);
        var completedAt = InitialTime.AddMinutes(4);

        record.Activate(activatedAt);
        record.RequestAiAnalysis(requestedAt);
        record.StartAiAnalysis(startedAt);
        record.CompleteAiAnalysis(
            "Resumen persistido",
            "Operaciones",
            "Revisar seguimiento",
            completedAt);

        repository.Add(record);
        await unitOfWork.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var persisted = await repository.GetByIdAsync(record.Id);

        Assert.NotNull(persisted);
        Assert.Equal(KnowledgeRecordStatus.Active, persisted.Status);
        Assert.Equal(AiProcessingStatus.Completed, persisted.AiStatus);
        Assert.Equal("Resumen persistido", persisted.Summary);
        Assert.Equal("Operaciones", persisted.Category);
        Assert.Equal("Revisar seguimiento", persisted.Recommendations);
        Assert.Null(persisted.AiError);
        Assert.Equal(InitialTime, persisted.CreatedAtUtc);
        Assert.Equal(completedAt, persisted.UpdatedAtUtc);
        Assert.Equal(completedAt, persisted.AiProcessedAtUtc);
    }

    private static KnowledgeRecord CreateRecord()
    {
        return KnowledgeRecord.Create(
            $"Integration record {Guid.NewGuid():N}",
            "Physical SQL Server repository validation.",
            "Integration tests",
            KnowledgeRecordType.BusinessRecord);
    }
}
