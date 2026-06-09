using KnowledgeAssistant.Application.Abstractions;

namespace KnowledgeAssistant.Application.Records;

public interface IGetKnowledgeRecordByIdHandler
{
    Task<KnowledgeRecordResult?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetKnowledgeRecordByIdHandler(
    IKnowledgeRecordRepository repository) : IGetKnowledgeRecordByIdHandler
{
    public async Task<KnowledgeRecordResult?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var record = await repository.GetByIdAsync(id, cancellationToken);

        return record is null
            ? null
            : KnowledgeRecordResult.FromEntity(record);
    }
}
