using KnowledgeAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeAssistant.Infrastructure.Persistence;

public sealed class KnowledgeAssistantDbContext(DbContextOptions<KnowledgeAssistantDbContext> options)
    : DbContext(options)
{
    public DbSet<KnowledgeRecord> KnowledgeRecords => Set<KnowledgeRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var record = modelBuilder.Entity<KnowledgeRecord>();

        record.ToTable("KnowledgeRecords");
        record.HasKey(item => item.Id);

        record.Property(item => item.Title)
            .HasMaxLength(KnowledgeRecord.MaxTitleLength)
            .IsRequired();

        record.Property(item => item.Content)
            .HasMaxLength(KnowledgeRecord.MaxContentLength)
            .IsRequired();

        record.Property(item => item.Source)
            .HasMaxLength(KnowledgeRecord.MaxSourceLength);

        record.Property(item => item.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        record.Property(item => item.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        record.Property(item => item.AiStatus)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        record.Property(item => item.Category)
            .HasMaxLength(KnowledgeRecord.MaxCategoryLength);

        record.Property(item => item.Summary)
            .HasMaxLength(KnowledgeRecord.MaxSummaryLength);

        record.Property(item => item.Recommendations)
            .HasMaxLength(KnowledgeRecord.MaxRecommendationsLength);

        record.Property(item => item.AiError)
            .HasMaxLength(KnowledgeRecord.MaxAiErrorLength);

        record.HasIndex(item => item.CreatedAtUtc);
        record.HasIndex(item => item.Category);
        record.HasIndex(item => item.Status);
        record.HasIndex(item => item.Type);
        record.HasIndex(item => item.AiStatus);
    }
}
