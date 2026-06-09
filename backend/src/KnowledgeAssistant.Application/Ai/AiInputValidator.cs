namespace KnowledgeAssistant.Application.Ai;

public static class AiInputValidator
{
    public const int MaxQuestionLength = 1_000;

    public static string NormalizeQuestion(string? question, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question, parameterName);

        var normalized = question.Trim();
        if (normalized.Length > MaxQuestionLength)
        {
            throw new ArgumentException(
                $"{parameterName} cannot exceed {MaxQuestionLength} characters.",
                parameterName);
        }

        return normalized;
    }
}
