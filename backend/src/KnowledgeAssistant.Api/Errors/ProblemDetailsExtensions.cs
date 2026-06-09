using Microsoft.AspNetCore.Mvc;

namespace KnowledgeAssistant.Api.Errors;

public static class ProblemDetailsExtensions
{
    public static void AddTraceId(this ProblemDetails problemDetails, HttpContext httpContext)
    {
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
    }
}
