using KnowledgeAssistant.Api.Errors;
using KnowledgeAssistant.Application;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeAssistant.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
public sealed class DashboardController(
    IGetDashboardStatsHandler statsHandler) : ControllerBase
{
    [HttpGet("stats")]
    [ProducesResponseType<DashboardStatsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStatsResponse>> GetStats(
        CancellationToken cancellationToken)
    {
        var stats = await statsHandler.HandleAsync(cancellationToken);

        return Ok(DashboardStatsResponse.FromStats(stats));
    }
}

public sealed record DashboardStatsResponse(
    int TotalRecords,
    IReadOnlyDictionary<string, int> ByStatus,
    IReadOnlyDictionary<string, int> ByType,
    IReadOnlyDictionary<string, int> ByAiStatus)
{
    public static DashboardStatsResponse FromStats(
        Application.Abstractions.DashboardStats stats)
    {
        ArgumentNullException.ThrowIfNull(stats);

        return new DashboardStatsResponse(
            stats.TotalRecords,
            stats.ByStatus,
            stats.ByType,
            stats.ByAiStatus);
    }
}
