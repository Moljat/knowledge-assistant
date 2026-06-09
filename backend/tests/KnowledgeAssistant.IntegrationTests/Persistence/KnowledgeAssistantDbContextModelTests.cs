using KnowledgeAssistant.Domain.Entities;
using KnowledgeAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KnowledgeAssistant.IntegrationTests.Persistence;

public sealed class KnowledgeAssistantDbContextModelTests
{
    private static readonly DbContextOptions<KnowledgeAssistantDbContext> Options =
        new DbContextOptionsBuilder<KnowledgeAssistantDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Trusted_Connection=True")
            .Options;

    [Fact]
    public void KnowledgeRecord_UsesExpectedTableAndRequiredColumns()
    {
        using var context = new KnowledgeAssistantDbContext(Options);
        var entity = context.Model.FindEntityType(typeof(KnowledgeRecord));

        Assert.NotNull(entity);
        Assert.Equal("KnowledgeRecords", entity.GetTableName());
        Assert.False(entity.FindProperty(nameof(KnowledgeRecord.Title))!.IsNullable);
        Assert.False(entity.FindProperty(nameof(KnowledgeRecord.Content))!.IsNullable);
        Assert.Equal(
            KnowledgeRecord.MaxContentLength,
            entity.FindProperty(nameof(KnowledgeRecord.Content))!.GetMaxLength());
    }

    [Theory]
    [InlineData(nameof(KnowledgeRecord.Title), KnowledgeRecord.MaxTitleLength)]
    [InlineData(nameof(KnowledgeRecord.Content), KnowledgeRecord.MaxContentLength)]
    [InlineData(nameof(KnowledgeRecord.Source), KnowledgeRecord.MaxSourceLength)]
    [InlineData(nameof(KnowledgeRecord.Category), KnowledgeRecord.MaxCategoryLength)]
    [InlineData(nameof(KnowledgeRecord.Summary), KnowledgeRecord.MaxSummaryLength)]
    [InlineData(nameof(KnowledgeRecord.Recommendations), KnowledgeRecord.MaxRecommendationsLength)]
    [InlineData(nameof(KnowledgeRecord.AiError), KnowledgeRecord.MaxAiErrorLength)]
    public void KnowledgeRecord_ColumnLengthsMatchDomainLimits(
        string propertyName,
        int expectedMaxLength)
    {
        using var context = new KnowledgeAssistantDbContext(Options);
        var property = context.Model
            .FindEntityType(typeof(KnowledgeRecord))!
            .FindProperty(propertyName)!;

        Assert.Equal(expectedMaxLength, property.GetMaxLength());
    }

    [Theory]
    [InlineData(nameof(KnowledgeRecord.CreatedAtUtc))]
    [InlineData(nameof(KnowledgeRecord.UpdatedAtUtc))]
    [InlineData(nameof(KnowledgeRecord.ArchivedAtUtc))]
    [InlineData(nameof(KnowledgeRecord.AiProcessedAtUtc))]
    public void KnowledgeRecord_DateColumnsUseUtcSecondPrecision(string propertyName)
    {
        using var context = new KnowledgeAssistantDbContext(Options);
        var property = context.Model
            .FindEntityType(typeof(KnowledgeRecord))!
            .FindProperty(propertyName)!;

        Assert.Equal(0, property.GetPrecision());
    }

    [Theory]
    [InlineData(nameof(KnowledgeRecord.Type))]
    [InlineData(nameof(KnowledgeRecord.Status))]
    [InlineData(nameof(KnowledgeRecord.AiStatus))]
    public void KnowledgeRecord_StoresEnumsAsStrings(string propertyName)
    {
        using var context = new KnowledgeAssistantDbContext(Options);
        var property = context.Model
            .FindEntityType(typeof(KnowledgeRecord))!
            .FindProperty(propertyName)!;

        Assert.Equal(typeof(string), property.GetProviderClrType());
    }

    [Theory]
    [InlineData(nameof(KnowledgeRecord.Category))]
    [InlineData(nameof(KnowledgeRecord.Status))]
    [InlineData(nameof(KnowledgeRecord.Type))]
    [InlineData(nameof(KnowledgeRecord.AiStatus))]
    [InlineData(nameof(KnowledgeRecord.CreatedAtUtc))]
    public void KnowledgeRecord_HasExpectedIndex(string propertyName)
    {
        using var context = new KnowledgeAssistantDbContext(Options);
        var entity = context.Model.FindEntityType(typeof(KnowledgeRecord))!;

        Assert.Contains(
            entity.GetIndexes(),
            index => index.Properties.Count == 1
                && index.Properties[0].Name == propertyName);
    }

    [Fact]
    public void Migrations_ContainsInitialCreate()
    {
        using var context = new KnowledgeAssistantDbContext(Options);

        var migrations = context.Database.GetMigrations();

        Assert.Contains(
            migrations,
            migration => migration.EndsWith("_InitialCreate", StringComparison.Ordinal));
    }
}
