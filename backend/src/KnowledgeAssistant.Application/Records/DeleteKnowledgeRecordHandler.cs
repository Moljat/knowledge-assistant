using KnowledgeAssistant.Application.Abstractions;

namespace KnowledgeAssistant.Application.Records;

public interface IDeleteKnowledgeRecordHandler
{
    Task<bool> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class DeleteKnowledgeRecordHandler(
    IKnowledgeRecordRepository repository,
    IUnitOfWork unitOfWork) : IDeleteKnowledgeRecordHandler
{
    public async Task<bool> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var record = await repository.GetByIdForUpdateAsync(id, cancellationToken);

        if (record is null)
        {
            return false;
        }

        repository.Remove(record);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
