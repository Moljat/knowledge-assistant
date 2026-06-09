using KnowledgeAssistant.Application.Abstractions;

namespace KnowledgeAssistant.Infrastructure.Persistence;

public sealed class UnitOfWork(KnowledgeAssistantDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
