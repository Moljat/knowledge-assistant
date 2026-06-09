using KnowledgeAssistant.Application.Abstractions;

namespace KnowledgeAssistant.Application.Records;

public interface IUpdateKnowledgeRecordHandler
{
    Task<KnowledgeRecordResult?> HandleAsync(
        UpdateKnowledgeRecordCommand command,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateKnowledgeRecordHandler(
    IKnowledgeRecordRepository repository,
    IUnitOfWork unitOfWork) : IUpdateKnowledgeRecordHandler
{
    public async Task<KnowledgeRecordResult?> HandleAsync(
        UpdateKnowledgeRecordCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var record = await repository.GetByIdForUpdateAsync(
            command.Id,
            cancellationToken);

        if (record is null)
        {
            return null;
        }

        record.Update(
            command.Title,
            command.Content,
            command.Source,
            command.Type);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return KnowledgeRecordResult.FromEntity(record);
    }
}
