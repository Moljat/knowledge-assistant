using KnowledgeAssistant.Application.Records;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeAssistant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateKnowledgeRecordHandler, CreateKnowledgeRecordHandler>();

        return services;
    }
}
