using System.Net;
using System.Net.Http.Json;
using KnowledgeAssistant.Api.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace KnowledgeAssistant.IntegrationTests.Api;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsVersionedHealthyResponse()
    {
        var response = await _client.GetAsync("/api/v1/system/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.NotNull(body);
        Assert.Equal("Healthy", body.Status);
        Assert.Equal("v1", body.ApiVersion);
    }
}
