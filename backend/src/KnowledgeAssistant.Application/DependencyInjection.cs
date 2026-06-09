using KnowledgeAssistant.Application.Ai;
using KnowledgeAssistant.Application.Records;
using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeAssistant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateKnowledgeRecordHandler, CreateKnowledgeRecordHandler>();
        services.AddScoped<IGetKnowledgeRecordByIdHandler, GetKnowledgeRecordByIdHandler>();
        services.AddScoped<IListKnowledgeRecordsHandler, ListKnowledgeRecordsHandler>();
        services.AddScoped<IUpdateKnowledgeRecordHandler, UpdateKnowledgeRecordHandler>();
        services.AddScoped<IDeleteKnowledgeRecordHandler, DeleteKnowledgeRecordHandler>();
        services.AddScoped<IGetDashboardStatsHandler, GetDashboardStatsHandler>();
        services.AddScoped<IAnalyzeRecordHandler, AnalyzeRecordHandler>();
        services.AddScoped<IChatHandler, ChatHandler>();

        return services;
    }
}
