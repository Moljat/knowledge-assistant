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
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        record.Property(item => item.Source)
            .HasMaxLength(KnowledgeRecord.MaxSourceLength);

        record.Property(item => item.Category)
            .HasMaxLength(100);

        record.Property(item => item.Summary)
            .HasColumnType("nvarchar(max)");

        record.Property(item => item.Recommendations)
            .HasColumnType("nvarchar(max)");

        record.HasIndex(item => item.CreatedAtUtc);
        record.HasIndex(item => item.Category);
    }
}
