using KnowledgeAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KnowledgeAssistant.Infrastructure.Persistence.Initialization;

public sealed class KnowledgeRecordSeeder(
    KnowledgeAssistantDbContext context,
    IOptions<SeedDataOptions> options,
    ILogger<KnowledgeRecordSeeder> logger)
{
    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Demo data seeding is disabled.");
            return 0;
        }

        var existingTitles = await context.KnowledgeRecords
            .AsNoTracking()
            .Where(record => record.Source == KnowledgeRecordSeedData.Source)
            .Select(record => record.Title)
            .ToHashSetAsync(cancellationToken);

        foreach (var definition in KnowledgeRecordSeedData.Records)
        {
            if (existingTitles.Contains(definition.Title))
            {
                continue;
            }

            var record = KnowledgeRecord.Create(
                definition.Title,
                definition.Content,
                KnowledgeRecordSeedData.Source,
                definition.Type);

            if (definition.Activate)
            {
                record.Activate();
            }

            context.KnowledgeRecords.Add(record);
        }

        var insertedRecords = await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Demo data seeding completed with {InsertedRecords} inserted records.",
            insertedRecords);

        return insertedRecords;
    }
}
