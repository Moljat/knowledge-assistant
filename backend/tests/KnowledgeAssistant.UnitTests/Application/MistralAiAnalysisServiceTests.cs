using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KnowledgeAssistant.Application.Ai;
using KnowledgeAssistant.Infrastructure.Ai;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace KnowledgeAssistant.UnitTests.Application;

public class MistralAiAnalysisServiceTests : IDisposable
{
    private readonly MockHttpMessageHandler _handler = new();
    private readonly MistralAiAnalysisService _service;

    public MistralAiAnalysisServiceTests()
    {
        var http = new HttpClient(_handler) { BaseAddress = new Uri("https://api.mistral.ai/v1/") };
        var options = Options.Create(new MistralOptions
        {
            BaseUrl = "https://api.mistral.ai/v1",
            Model = "mistral-small-latest",
            ApiKey = "test-key",
            TimeoutSeconds = 30
        });
        _service = new MistralAiAnalysisService(http, options, NullLogger<MistralAiAnalysisService>.Instance);
    }

    public void Dispose() => _handler.Dispose();

    [Fact]
    public async Task AnalyzeAsync_ShouldSendCorrectRequest()
    {
        _handler.Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = """{"summary": "Test summary"}"""
                        }
                    }
                }
            })
        };

        var result = await _service.AnalyzeAsync(
            new AiAnalysisRequest("Content", AiAnalysisType.Summary));

        Assert.NotNull(result);
        Assert.Equal("Test summary", result.Summary);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldReturnEmptyOnMissingContent()
    {
        _handler.Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                choices = new[]
                {
                    new { message = new { content = (string?)null } }
                }
            })
        };

        var result = await _service.AnalyzeAsync(
            new AiAnalysisRequest("Content", AiAnalysisType.Summary));

        Assert.NotNull(result);
        Assert.Null(result.Summary);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldParseClassification()
    {
        _handler.Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                choices = new[]
                {
                    new { message = new { content = """{"category": "Tecnologia"}""" } }
                }
            })
        };

        var result = await _service.AnalyzeAsync(
            new AiAnalysisRequest("Tech content", AiAnalysisType.Classification));

        Assert.Equal("Tecnologia", result.Category);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldParseRecommendations()
    {
        _handler.Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = """{"recommendations": ["Rec1", "Rec2"]}"""
                        }
                    }
                }
            })
        };

        var result = await _service.AnalyzeAsync(
            new AiAnalysisRequest("Content", AiAnalysisType.Recommendations));

        Assert.Equal(2, result.Recommendations.Count);
        Assert.Contains("Rec1", result.Recommendations);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldParseQuestion()
    {
        _handler.Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                choices = new[]
                {
                    new { message = new { content = """{"answer": "42"}""" } }
                }
            })
        };

        var result = await _service.AnalyzeAsync(
            new AiAnalysisRequest("Life context", AiAnalysisType.Question, "What?"));

        Assert.Equal("42", result.Answer);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldThrowOnMalformedWrapperJson()
    {
        _handler.Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not json")
        };

        await Assert.ThrowsAsync<JsonException>(() =>
            _service.AnalyzeAsync(new AiAnalysisRequest("Content", AiAnalysisType.Summary)));
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldThrowOnHttpError()
    {
        _handler.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _service.AnalyzeAsync(new AiAnalysisRequest("Content", AiAnalysisType.Summary)));
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldReturnEmptyOnEmptyResponse()
    {
        _handler.Response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { choices = Array.Empty<object>() })
        };

        var result = await _service.AnalyzeAsync(
            new AiAnalysisRequest("Content", AiAnalysisType.Summary));

        Assert.Null(result.Summary);
        Assert.Null(result.Category);
        Assert.Empty(result.Recommendations);
        Assert.Null(result.Answer);
    }

    private sealed class MockHttpMessageHandler : DelegatingHandler
    {
        public HttpResponseMessage? Response { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Response
                ?? new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
