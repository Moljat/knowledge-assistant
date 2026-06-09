using KnowledgeAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KnowledgeAssistant.Infrastructure.Persistence.Configurations;

public sealed class KnowledgeRecordConfiguration : IEntityTypeConfiguration<KnowledgeRecord>
{
    public void Configure(EntityTypeBuilder<KnowledgeRecord> builder)
    {
        builder.ToTable("KnowledgeRecords");
        builder.HasKey(record => record.Id);

        builder.Property(record => record.Title)
            .HasMaxLength(KnowledgeRecord.MaxTitleLength)
            .IsRequired();

        builder.Property(record => record.Content)
            .HasMaxLength(KnowledgeRecord.MaxContentLength)
            .IsRequired();

        builder.Property(record => record.Source)
            .HasMaxLength(KnowledgeRecord.MaxSourceLength);

        builder.Property(record => record.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(record => record.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(record => record.AiStatus)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(record => record.Category)
            .HasMaxLength(KnowledgeRecord.MaxCategoryLength);

        builder.Property(record => record.Summary)
            .HasMaxLength(KnowledgeRecord.MaxSummaryLength);

        builder.Property(record => record.Recommendations)
            .HasMaxLength(KnowledgeRecord.MaxRecommendationsLength);

        builder.Property(record => record.AiError)
            .HasMaxLength(KnowledgeRecord.MaxAiErrorLength);

        builder.Property(record => record.CreatedAtUtc)
            .HasPrecision(0);

        builder.Property(record => record.UpdatedAtUtc)
            .HasPrecision(0);

        builder.Property(record => record.ArchivedAtUtc)
            .HasPrecision(0);

        builder.Property(record => record.AiProcessedAtUtc)
            .HasPrecision(0);

        builder.Property(record => record.AiRetryCount)
            .HasDefaultValue(0);

        builder.HasIndex(record => record.CreatedAtUtc);
        builder.HasIndex(record => record.Category);
        builder.HasIndex(record => record.Status);
        builder.HasIndex(record => record.Type);
        builder.HasIndex(record => record.AiStatus);
    }
}
