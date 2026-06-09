using KnowledgeAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeAssistant.IntegrationTests.Persistence;

public sealed class SqlServerFixture : IAsyncLifetime
{
    public string? ConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        ConnectionString = Environment.GetEnvironmentVariable(
            SqlServerFactAttribute.ConnectionVariable);
        if (ConnectionString is null)
        {
            return;
        }

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public KnowledgeAssistantDbContext CreateContext()
    {
        var connectionString = ConnectionString
            ?? throw new InvalidOperationException(
                "SQL Server fixture was used without a configured connection.");

        var options = new DbContextOptionsBuilder<KnowledgeAssistantDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new KnowledgeAssistantDbContext(options);
    }
}
