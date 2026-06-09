using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Application.Ai;
using KnowledgeAssistant.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KnowledgeAssistant.Infrastructure.BackgroundServices;

public sealed class AiProcessingBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<AiProcessingBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(30);
    private const int BatchSize = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("AI processing background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingRecordsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Unexpected error in AI processing background service.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingRecordsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IKnowledgeRecordRepository>();
        var aiService = scope.ServiceProvider.GetRequiredService<IAiAnalysisService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pendingRecords = await repository.ListAsync(
            new KnowledgeRecordListFilters(
                null, null, null, null, AiProcessingStatus.Pending),
            1,
            BatchSize,
            cancellationToken);

        foreach (var record in pendingRecords.Items)
        {
            if (cancellationToken.IsCancellationRequested) break;

            await ProcessRecordAsync(record, repository, aiService, unitOfWork, cancellationToken);
        }
    }

    private static async Task ProcessRecordAsync(
        KnowledgeRecord record,
        IKnowledgeRecordRepository repository,
        IAiAnalysisService aiService,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var tracked = await repository.GetByIdForUpdateAsync(record.Id, cancellationToken);
        if (tracked is null) return;

        try
        {
            tracked.StartAiAnalysis();

            var result = await aiService.AnalyzeAsync(
                new AiAnalysisRequest(tracked.Content, AiAnalysisType.Summary),
                cancellationToken);

            var recommendations = result.Recommendations.Count > 0
                ? string.Join("\n", result.Recommendations)
                : null;

            tracked.CompleteAiAnalysis(result.Summary, result.Category, recommendations);
        }
        catch (Exception exception)
        {
            tracked.FailAiAnalysis(exception.Message);
        }
        finally
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
