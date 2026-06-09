using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Infrastructure.Ai;
using KnowledgeAssistant.Infrastructure.BackgroundServices;
using KnowledgeAssistant.Infrastructure.Persistence;
using KnowledgeAssistant.Infrastructure.Persistence.Initialization;
using KnowledgeAssistant.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KnowledgeAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<KnowledgeAssistantDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServer => sqlServer.MigrationsAssembly(
                    typeof(KnowledgeAssistantDbContext).Assembly.FullName)));

        services.AddScoped<IKnowledgeRecordRepository, KnowledgeRecordRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<KnowledgeRecordSeeder>();

        services
            .AddOptions<DatabaseInitializationOptions>()
            .Bind(configuration.GetSection(DatabaseInitializationOptions.SectionName));

        services
            .AddOptions<SeedDataOptions>()
            .Bind(configuration.GetSection(SeedDataOptions.SectionName));

        services
            .AddOptions<MistralOptions>()
            .Bind(configuration.GetSection(MistralOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(
                    options.BaseUrl,
                    UriKind.Absolute,
                    out var uri)
                    && uri.Scheme == Uri.UriSchemeHttps,
                "Mistral:BaseUrl must be an absolute HTTPS URL.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Model),
                "Mistral:Model is required.")
            .Validate(
                options => options.TimeoutSeconds is >= 1 and <= 120,
                "Mistral:TimeoutSeconds must be between 1 and 120.")
            .ValidateOnStart();

        services.AddTransient<MistralResilienceHandler>();
        services.AddHostedService<AiProcessingBackgroundService>();

        services.AddHttpClient<IAiAnalysisService, MistralAiAnalysisService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MistralOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
        }).AddHttpMessageHandler<MistralResilienceHandler>();

        return services;
    }
}
