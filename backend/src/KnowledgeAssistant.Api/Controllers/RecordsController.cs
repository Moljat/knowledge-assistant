using KnowledgeAssistant.Application.Records;
using KnowledgeAssistant.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeAssistant.Api.Controllers;

[ApiController]
[Route("api/v1/records")]
public sealed class RecordsController(
    ICreateKnowledgeRecordHandler createHandler,
    IGetKnowledgeRecordByIdHandler getByIdHandler,
    IListKnowledgeRecordsHandler listHandler,
    IUpdateKnowledgeRecordHandler updateHandler) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<KnowledgeRecordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KnowledgeRecordResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var record = await getByIdHandler.HandleAsync(id, cancellationToken);

        if (record is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Knowledge record not found.",
                Detail = $"No knowledge record exists with id '{id}'."
            });
        }

        return Ok(KnowledgeRecordResponse.FromResult(record));
    }

    [HttpGet]
    [ProducesResponseType<PagedKnowledgeRecordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedKnowledgeRecordResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var records = await listHandler.HandleAsync(
                new ListKnowledgeRecordsQuery(page, pageSize),
                cancellationToken);

            return Ok(PagedKnowledgeRecordResponse.FromResult(records));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            ModelState.AddModelError(exception.ParamName ?? "pagination", exception.Message);
            return ValidationProblem(ModelState);
        }
    }

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

    [HttpPut("{id:guid}")]
    [ProducesResponseType<KnowledgeRecordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KnowledgeRecordResponse>> Update(
        Guid id,
        UpdateKnowledgeRecordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var record = await updateHandler.HandleAsync(
                new UpdateKnowledgeRecordCommand(
                    id,
                    request.Title,
                    request.Content,
                    request.Source,
                    request.Type),
                cancellationToken);

            if (record is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Knowledge record not found.",
                    Detail = $"No knowledge record exists with id '{id}'."
                });
            }

            return Ok(KnowledgeRecordResponse.FromResult(record));
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(exception.ParamName ?? "request", exception.Message);
            return ValidationProblem(ModelState);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Knowledge record cannot be updated.",
                Detail = exception.Message
            });
        }
    }
}

public sealed record PagedKnowledgeRecordResponse(
    IReadOnlyList<KnowledgeRecordResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages)
{
    public static PagedKnowledgeRecordResponse FromResult(
        PagedResult<KnowledgeRecordResult> result)
    {
        return new PagedKnowledgeRecordResponse(
            result.Items
                .Select(KnowledgeRecordResponse.FromResult)
                .ToList(),
            result.Page,
            result.PageSize,
            result.TotalItems,
            result.TotalPages);
    }
}

public sealed record CreateKnowledgeRecordRequest(
    string Title,
    string Content,
    string? Source,
    KnowledgeRecordType Type);

public sealed record UpdateKnowledgeRecordRequest(
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
