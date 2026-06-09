using KnowledgeAssistant.Domain.Entities;
using KnowledgeAssistant.Infrastructure.Persistence;
using KnowledgeAssistant.Infrastructure.Persistence.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace KnowledgeAssistant.IntegrationTests.Persistence;

[Collection(SqlServerCollection.Name)]
public sealed class KnowledgeRecordSeederTests(SqlServerFixture fixture)
{
    [SqlServerFact]
    public async Task SeedAsync_FirstExecution_InsertsDemoRecordsWithoutAiResults()
    {
        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var seeder = CreateSeeder(context, enabled: true);

        var insertedRecords = await seeder.SeedAsync();
        var records = await GetSeedRecordsAsync(context);

        Assert.Equal(KnowledgeRecordSeedData.Records.Count, insertedRecords);
        Assert.Equal(KnowledgeRecordSeedData.Records.Count, records.Count);
        Assert.Contains(records, record => record.Status == KnowledgeRecordStatus.Draft);
        Assert.Contains(records, record => record.Status == KnowledgeRecordStatus.Active);
        Assert.All(records, record =>
        {
            Assert.Equal(AiProcessingStatus.NotRequested, record.AiStatus);
            Assert.Null(record.Summary);
            Assert.Null(record.Category);
            Assert.Null(record.Recommendations);
        });
    }

    [SqlServerFact]
    public async Task SeedAsync_SecondExecution_DoesNotCreateDuplicates()
    {
        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var seeder = CreateSeeder(context, enabled: true);

        var firstExecution = await seeder.SeedAsync();
        context.ChangeTracker.Clear();
        var secondExecution = await seeder.SeedAsync();
        var records = await GetSeedRecordsAsync(context);

        Assert.Equal(KnowledgeRecordSeedData.Records.Count, firstExecution);
        Assert.Equal(0, secondExecution);
        Assert.Equal(KnowledgeRecordSeedData.Records.Count, records.Count);
    }

    [SqlServerFact]
    public async Task SeedAsync_WhenDisabled_DoesNotInsertRecords()
    {
        await using var context = fixture.CreateContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var seeder = CreateSeeder(context, enabled: false);

        var insertedRecords = await seeder.SeedAsync();
        var records = await GetSeedRecordsAsync(context);

        Assert.Equal(0, insertedRecords);
        Assert.Empty(records);
    }

    private static KnowledgeRecordSeeder CreateSeeder(
        KnowledgeAssistantDbContext context,
        bool enabled)
    {
        return new KnowledgeRecordSeeder(
            context,
            Options.Create(new SeedDataOptions { Enabled = enabled }),
            NullLogger<KnowledgeRecordSeeder>.Instance);
    }

    private static Task<List<KnowledgeRecord>> GetSeedRecordsAsync(
        KnowledgeAssistantDbContext context)
    {
        return context.KnowledgeRecords
            .AsNoTracking()
            .Where(record => record.Source == KnowledgeRecordSeedData.Source)
            .OrderBy(record => record.Title)
            .ToListAsync();
    }
}
