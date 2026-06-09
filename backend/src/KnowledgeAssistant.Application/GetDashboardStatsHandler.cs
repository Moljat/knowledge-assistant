using KnowledgeAssistant.Application.Abstractions;

namespace KnowledgeAssistant.Application;

public interface IGetDashboardStatsHandler
{
    Task<DashboardStats> HandleAsync(CancellationToken cancellationToken = default);
}

public sealed class GetDashboardStatsHandler(
    IKnowledgeRecordRepository repository) : IGetDashboardStatsHandler
{
    public async Task<DashboardStats> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await repository.GetDashboardStatsAsync(cancellationToken);
    }
}
