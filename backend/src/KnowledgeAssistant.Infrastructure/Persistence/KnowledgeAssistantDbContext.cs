using KnowledgeAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeAssistant.Infrastructure.Persistence;

public sealed class KnowledgeAssistantDbContext(DbContextOptions<KnowledgeAssistantDbContext> options)
    : DbContext(options)
{
    public DbSet<KnowledgeRecord> KnowledgeRecords => Set<KnowledgeRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KnowledgeAssistantDbContext).Assembly);
    }
}
