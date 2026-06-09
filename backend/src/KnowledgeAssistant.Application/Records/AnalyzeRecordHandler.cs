using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Application.Ai;

namespace KnowledgeAssistant.Application.Records;

public interface IAnalyzeRecordHandler
{
    Task<KnowledgeRecordResult?> HandleAsync(
        AnalyzeRecordCommand command,
        CancellationToken cancellationToken = default);
}

public sealed class AnalyzeRecordHandler(
    IKnowledgeRecordRepository repository,
    IAiAnalysisService aiService,
    IUnitOfWork unitOfWork) : IAnalyzeRecordHandler
{
    public async Task<KnowledgeRecordResult?> HandleAsync(
        AnalyzeRecordCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var record = await repository.GetByIdAsync(command.RecordId, cancellationToken);

        if (record is null) return null;

        record.RequestAiAnalysis();

        try
        {
            var result = await aiService.AnalyzeAsync(
                new AiAnalysisRequest(
                    record.Content,
                    command.Type,
                    command.Question),
                cancellationToken);

            var recommendations = result.Recommendations.Count > 0
                ? string.Join("\n", result.Recommendations)
                : null;

            record.CompleteAiAnalysis(
                result.Summary,
                result.Category,
                recommendations);
        }
        catch (Exception exception)
        {
            record.FailAiAnalysis(exception.Message);
            throw;
        }
        finally
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return KnowledgeRecordResult.FromEntity(record);
    }
}
