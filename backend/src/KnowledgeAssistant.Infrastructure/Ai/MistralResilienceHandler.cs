using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace KnowledgeAssistant.Infrastructure.Ai;

public sealed class MistralResilienceHandler : DelegatingHandler
{
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;
    private readonly ILogger<MistralResilienceHandler> _logger;

    public MistralResilienceHandler(ILogger<MistralResilienceHandler> logger)
    {
        _logger = logger;
        _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = args => ValueTask.FromResult(ShouldRetry(args.Outcome)),
                OnRetry = args =>
                {
                    var statusCode = args.Outcome.Result?.StatusCode;
                    _logger.LogWarning(
                        "Mistral call failed with {StatusCode}. Retry {Attempt} after {Delay}ms.",
                        statusCode,
                        args.AttemptNumber + 1,
                        args.RetryDelay.TotalMilliseconds);

                    return ValueTask.CompletedTask;
                },
            })
            .Build();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await _pipeline.ExecuteAsync(
            async ct => await base.SendAsync(request, ct),
            cancellationToken);
    }

    private static bool ShouldRetry(Outcome<HttpResponseMessage> outcome)
    {
        if (outcome.Exception is not null)
        {
            return outcome.Exception is HttpRequestException or TaskCanceledException;
        }

        var statusCode = outcome.Result?.StatusCode;
        return statusCode is HttpStatusCode.TooManyRequests
            or >= HttpStatusCode.InternalServerError;
    }
}
