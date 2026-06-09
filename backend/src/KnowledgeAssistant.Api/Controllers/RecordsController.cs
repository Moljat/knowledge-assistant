using KnowledgeAssistant.Application.Records;
using KnowledgeAssistant.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeAssistant.Api.Controllers;

[ApiController]
[Route("api/v1/records")]
public sealed class RecordsController(
    ICreateKnowledgeRecordHandler createHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<KnowledgeRecordResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<KnowledgeRecordResponse>> Create(
        CreateKnowledgeRecordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var record = await createHandler.HandleAsync(
                new CreateKnowledgeRecordCommand(
                    request.Title,
                    request.Content,
                    request.Source,
                    request.Type),
                cancellationToken);

            var response = KnowledgeRecordResponse.FromResult(record);
            return Created($"/api/v1/records/{response.Id}", response);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(exception.ParamName ?? "request", exception.Message);
            return ValidationProblem(ModelState);
        }
    }
}

public sealed record CreateKnowledgeRecordRequest(
    string Title,
    string Content,
    string? Source,
    KnowledgeRecordType Type);

public sealed record KnowledgeRecordResponse(
    Guid Id,
    string Title,
    string Content,
    string? Source,
    KnowledgeRecordType Type,
    KnowledgeRecordStatus Status,
    AiProcessingStatus AiStatus,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc)
{
    public static KnowledgeRecordResponse FromResult(KnowledgeRecordResult result)
    {
        return new KnowledgeRecordResponse(
            result.Id,
            result.Title,
            result.Content,
            result.Source,
            result.Type,
            result.Status,
            result.AiStatus,
            result.CreatedAtUtc,
            result.UpdatedAtUtc);
    }
}
