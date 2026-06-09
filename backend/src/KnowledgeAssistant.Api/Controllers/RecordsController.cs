using KnowledgeAssistant.Api.Errors;
using KnowledgeAssistant.Application.Ai;
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
    IUpdateKnowledgeRecordHandler updateHandler,
    IDeleteKnowledgeRecordHandler deleteHandler,
    IAnalyzeRecordHandler analyzeHandler,
    ILogger<RecordsController> logger) : ControllerBase
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
            return NotFound(CreateNotFoundProblem(id));
        }

        return Ok(KnowledgeRecordResponse.FromResult(record));
    }

    [HttpGet]
    [ProducesResponseType<PagedKnowledgeRecordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedKnowledgeRecordResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] KnowledgeRecordStatus? status = null,
        [FromQuery] KnowledgeRecordType? type = null,
        [FromQuery] AiProcessingStatus? aiStatus = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var records = await listHandler.HandleAsync(
                new ListKnowledgeRecordsQuery(
                    page,
                    pageSize,
                    search,
                    category,
                    status,
                    type,
                    aiStatus),
                cancellationToken);

            return Ok(PagedKnowledgeRecordResponse.FromResult(records));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            logger.LogWarning(
                exception,
                "Rejected record listing because pagination is invalid.");
            return CreateValidationProblem(exception, "pagination");
        }
        catch (ArgumentException exception)
        {
            logger.LogWarning(
                exception,
                "Rejected record listing because filters are invalid.");
            return CreateValidationProblem(exception, "filters");
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
            logger.LogInformation("Created knowledge record {RecordId}.", response.Id);
            return Created($"/api/v1/records/{response.Id}", response);
        }
        catch (ArgumentException exception)
        {
            logger.LogWarning(exception, "Rejected record creation because payload is invalid.");
            return CreateValidationProblem(exception, "request");
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
                return NotFound(CreateNotFoundProblem(id));
            }

            logger.LogInformation("Updated knowledge record {RecordId}.", id);
            return Ok(KnowledgeRecordResponse.FromResult(record));
        }
        catch (ArgumentException exception)
        {
            logger.LogWarning(
                exception,
                "Rejected update for knowledge record {RecordId} because payload is invalid.",
                id);
            return CreateValidationProblem(exception, "request");
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(
                exception,
                "Rejected update for knowledge record {RecordId} because a business rule failed.",
                id);
            return BadRequest(CreateBusinessRuleProblem(
                "Knowledge record cannot be updated.",
                exception.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await deleteHandler.HandleAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound(CreateNotFoundProblem(id));
        }

        logger.LogInformation("Deleted knowledge record {RecordId}.", id);
        return NoContent();
    }

    [HttpPost("{id:guid}/ai/summary")]
    [ProducesResponseType<KnowledgeRecordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<KnowledgeRecordResponse>> AnalyzeSummary(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await Analyze(id, AiAnalysisType.Summary, null, cancellationToken);
    }

    [HttpPost("{id:guid}/ai/classification")]
    [ProducesResponseType<KnowledgeRecordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<KnowledgeRecordResponse>> AnalyzeClassification(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await Analyze(id, AiAnalysisType.Classification, null, cancellationToken);
    }

    [HttpPost("{id:guid}/ai/recommendations")]
    [ProducesResponseType<KnowledgeRecordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<KnowledgeRecordResponse>> AnalyzeRecommendations(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await Analyze(id, AiAnalysisType.Recommendations, null, cancellationToken);
    }

    [HttpPost("{id:guid}/ai/questions")]
    [ProducesResponseType<KnowledgeRecordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<KnowledgeRecordResponse>> AnalyzeQuestion(
        Guid id,
        AiQuestionRequest request,
        CancellationToken cancellationToken)
    {
        return await Analyze(id, AiAnalysisType.Question, request.Question, cancellationToken);
    }

    private async Task<ActionResult<KnowledgeRecordResponse>> Analyze(
        Guid id,
        AiAnalysisType type,
        string? question,
        CancellationToken cancellationToken)
    {
        try
        {
            var record = await analyzeHandler.HandleAsync(
                new AnalyzeRecordCommand(id, type, question),
                cancellationToken);

            if (record is null)
            {
                return NotFound(CreateNotFoundProblem(id));
            }

            logger.LogInformation(
                "Completed {Type} analysis for record {RecordId}.",
                type,
                id);

            return Ok(KnowledgeRecordResponse.FromResult(record));
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(
                exception,
                "Rejected analysis for record {RecordId} because a business rule failed.",
                id);

            return BadRequest(CreateBusinessRuleProblem(
                "AI analysis cannot be performed.",
                exception.Message));
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                exception,
                "AI service request failed for record {RecordId}.",
                id);

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ProblemDetails
                {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "AI service unavailable.",
                    Type = ApiProblemTypes.ServiceUnavailable,
                    Detail = "The AI analysis service is temporarily unavailable."
                });
        }
    }

    private ActionResult CreateValidationProblem(
        ArgumentException exception,
        string fallbackKey)
    {
        var key = exception.ParamName ?? fallbackKey;
        var problem = new ValidationProblemDetails(
            new Dictionary<string, string[]>
            {
                [key] = [exception.Message]
            })
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Type = ApiProblemTypes.Validation
        };
        problem.AddTraceId(HttpContext);

        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    }

    private ProblemDetails CreateNotFoundProblem(Guid id)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Knowledge record not found.",
            Type = ApiProblemTypes.NotFound,
            Detail = $"No knowledge record exists with id '{id}'."
        };
        problem.AddTraceId(HttpContext);

        return problem;
    }

    private ProblemDetails CreateBusinessRuleProblem(string title, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = title,
            Type = ApiProblemTypes.BusinessRule,
            Detail = detail
        };
        problem.AddTraceId(HttpContext);

        return problem;
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

public sealed record AiQuestionRequest(string Question);

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
