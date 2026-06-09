using KnowledgeAssistant.Api.Errors;
using KnowledgeAssistant.Application.Ai;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeAssistant.Api.Controllers;

[ApiController]
[Route("api/v1/ai")]
public sealed class AiController(
    IChatHandler chatHandler,
    ILogger<AiController> logger) : ControllerBase
{
    [HttpPost("chat")]
    [ProducesResponseType<ChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ChatResponse>> Chat(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await chatHandler.HandleAsync(request, cancellationToken);

            logger.LogInformation(
                "Completed general chat. Question length: {Length}.",
                request.Question.Length);

            return Ok(response);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "AI service request failed for general chat.");

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ProblemDetails
                {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "AI service unavailable.",
                    Type = ApiProblemTypes.ServiceUnavailable,
                    Detail = "The AI chat service is temporarily unavailable."
                });
        }
    }
}
