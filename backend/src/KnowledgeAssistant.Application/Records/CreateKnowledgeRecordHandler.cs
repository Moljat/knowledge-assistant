using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Domain.Entities;

namespace KnowledgeAssistant.Application.Records;

public interface ICreateKnowledgeRecordHandler
{
    Task<KnowledgeRecordResult> HandleAsync(
        CreateKnowledgeRecordCommand command,
        CancellationToken cancellationToken = default);
}

public sealed class CreateKnowledgeRecordHandler(
    IKnowledgeRecordRepository repository,
    IUnitOfWork unitOfWork) : ICreateKnowledgeRecordHandler
{
    public async Task<KnowledgeRecordResult> HandleAsync(
        CreateKnowledgeRecordCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var record = KnowledgeRecord.Create(
            command.Title,
            command.Content,
            command.Source,
            command.Type);

        repository.Add(record);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return KnowledgeRecordResult.FromEntity(record);
    }
}
