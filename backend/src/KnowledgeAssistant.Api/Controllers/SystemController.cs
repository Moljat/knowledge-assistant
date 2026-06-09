using Microsoft.AspNetCore.Mvc;

namespace KnowledgeAssistant.Api.Controllers;

[ApiController]
[Route("api/v1/system")]
public sealed class SystemController : ControllerBase
{
    [HttpGet("health")]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> GetHealth()
    {
        return Ok(new HealthResponse(
            "Knowledge Assistant API",
            "Healthy",
            "v1",
            DateTimeOffset.UtcNow));
    }
}

public sealed record HealthResponse(
    string Service,
    string Status,
    string ApiVersion,
    DateTimeOffset TimestampUtc);
