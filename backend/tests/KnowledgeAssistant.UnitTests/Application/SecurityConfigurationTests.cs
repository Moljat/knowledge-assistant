using KnowledgeAssistant.Infrastructure;
using KnowledgeAssistant.Infrastructure.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KnowledgeAssistant.UnitTests.Application;

public sealed class SecurityConfigurationTests
{
    [Theory]
    [InlineData("http://api.mistral.ai/v1", "mistral-small-latest", "30")]
    [InlineData("not-a-url", "mistral-small-latest", "30")]
    [InlineData("https://api.mistral.ai/v1", " ", "30")]
    [InlineData("https://api.mistral.ai/v1", "mistral-small-latest", "0")]
    [InlineData("https://api.mistral.ai/v1", "mistral-small-latest", "121")]
    public void AddInfrastructure_WithInvalidMistralConfiguration_FailsValidation(
        string baseUrl,
        string model,
        string timeoutSeconds)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(BuildConfiguration(baseUrl, model, timeoutSeconds));
        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<MistralOptions>>().Value);
    }

    [Fact]
    public void AddInfrastructure_WithSecureMistralConfiguration_BindsOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(BuildConfiguration(
            "https://api.mistral.ai/v1",
            "mistral-small-latest",
            "30"));
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<MistralOptions>>().Value;

        Assert.Equal("https://api.mistral.ai/v1", options.BaseUrl);
        Assert.Equal(30, options.TimeoutSeconds);
    }

    private static IConfiguration BuildConfiguration(
        string baseUrl,
        string model,
        string timeoutSeconds)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=(localdb)\\MSSQLLocalDB;Database=SecurityTests;Trusted_Connection=True",
                ["Mistral:BaseUrl"] = baseUrl,
                ["Mistral:Model"] = model,
                ["Mistral:ApiKey"] = "test-key",
                ["Mistral:TimeoutSeconds"] = timeoutSeconds
            })
            .Build();
    }
}
