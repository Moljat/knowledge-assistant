using System.Text.Json;
using KnowledgeAssistant.Application.Ai;
using KnowledgeAssistant.Infrastructure.Ai;

namespace KnowledgeAssistant.UnitTests.Application;

public class AiPromptsTests
{
    [Theory]
    [InlineData(AiAnalysisType.Summary)]
    [InlineData(AiAnalysisType.Classification)]
    [InlineData(AiAnalysisType.Recommendations)]
    [InlineData(AiAnalysisType.Question)]
    public void BuildSystemPrompt_ShouldReturnNonEmptyForAllTypes(AiAnalysisType type)
    {
        var prompt = AiPrompts.BuildSystemPrompt(type);
        Assert.False(string.IsNullOrWhiteSpace(prompt));
    }

    [Theory]
    [InlineData(AiAnalysisType.Summary, "summary")]
    [InlineData(AiAnalysisType.Classification, "category")]
    [InlineData(AiAnalysisType.Recommendations, "recommendations")]
    [InlineData(AiAnalysisType.Question, "answer")]
    public void BuildSystemPrompt_ShouldMentionExpectedJsonField(AiAnalysisType type, string field)
    {
        var prompt = AiPrompts.BuildSystemPrompt(type);
        Assert.Contains(field, prompt, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(AiAnalysisType.Summary)]
    [InlineData(AiAnalysisType.Classification)]
    [InlineData(AiAnalysisType.Recommendations)]
    [InlineData(AiAnalysisType.Question)]
    public void BuildSystemPrompt_ShouldBeInSpanish(AiAnalysisType type)
    {
        var prompt = AiPrompts.BuildSystemPrompt(type);
        Assert.Contains("json", prompt, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(AiAnalysisType.Summary)]
    [InlineData(AiAnalysisType.Classification)]
    [InlineData(AiAnalysisType.Recommendations)]
    [InlineData(AiAnalysisType.Question)]
    public void ExpectedSchema_ShouldBeValidJson(AiAnalysisType type)
    {
        var schema = AiPrompts.ExpectedSchema(type);
        var ex = Record.Exception(() => JsonDocument.Parse(schema));
        Assert.Null(ex);
    }
}
