using KnowledgeAssistant.Infrastructure;
using KnowledgeAssistant.Infrastructure.Persistence;
using KnowledgeAssistant.Infrastructure.Persistence.Initialization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeAssistant.IntegrationTests.Persistence;

[Collection(SqlServerCollection.Name)]
public sealed class DatabaseInitializerTests(SqlServerFixture fixture)
{
    [SqlServerFact]
    public async Task InitializeAsync_MigratesAndSeedsTemporaryDatabaseIdempotently()
    {
        var connectionBuilder = new SqlConnectionStringBuilder(fixture.ConnectionString)
        {
            InitialCatalog = $"KnowledgeAssistantSeed_{Guid.NewGuid():N}"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionBuilder.ConnectionString,
                ["DatabaseInitialization:Enabled"] = "true",
                ["SeedData:Enabled"] = "true"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        var context = scope.ServiceProvider.GetRequiredService<KnowledgeAssistantDbContext>();

        try
        {
            await initializer.InitializeAsync();
            await initializer.InitializeAsync();

            var migrations = await context.Database.GetAppliedMigrationsAsync();
            var seedRecords = await context.KnowledgeRecords
                .AsNoTracking()
                .Where(record => record.Source == KnowledgeRecordSeedData.Source)
                .ToListAsync();

            Assert.Contains(
                migrations,
                migration => migration.EndsWith("_InitialCreate", StringComparison.Ordinal));
            Assert.Equal(KnowledgeRecordSeedData.Records.Count, seedRecords.Count);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
}
