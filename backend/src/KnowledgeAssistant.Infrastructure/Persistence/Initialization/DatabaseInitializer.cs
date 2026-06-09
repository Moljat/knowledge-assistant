using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KnowledgeAssistant.Infrastructure.Persistence.Initialization;

public sealed class DatabaseInitializer(
    KnowledgeAssistantDbContext context,
    KnowledgeRecordSeeder seeder,
    IOptions<DatabaseInitializationOptions> initializationOptions,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!initializationOptions.Value.Enabled)
        {
            logger.LogInformation("Automatic database initialization is disabled.");
            return;
        }

        logger.LogInformation("Applying pending database migrations.");
        await context.Database.MigrateAsync(cancellationToken);

        await seeder.SeedAsync(cancellationToken);
    }
}
