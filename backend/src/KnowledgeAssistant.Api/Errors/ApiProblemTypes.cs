namespace KnowledgeAssistant.Api.Errors;

public static class ApiProblemTypes
{
    public const string Validation = "/problems/validation-error";
    public const string NotFound = "/problems/not-found";
    public const string BusinessRule = "/problems/business-rule-violation";
    public const string InternalServerError = "/problems/internal-server-error";
    public const string ServiceUnavailable = "/problems/service-unavailable";
}
