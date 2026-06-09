using KnowledgeAssistant.Application;
using KnowledgeAssistant.Api.Errors;
using KnowledgeAssistant.Infrastructure;
using KnowledgeAssistant.Infrastructure.Persistence.Initialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        if (context.ProblemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            context.ProblemDetails.Type = ApiProblemTypes.InternalServerError;
            context.ProblemDetails.Title = "An unexpected error occurred.";
        }
    };
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Type = ApiProblemTypes.Validation
        };
        problem.AddTraceId(context.HttpContext);

        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var databaseInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await databaseInitializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler("/error");
app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

app.Map("/error", (HttpContext httpContext, ILogger<Program> logger) =>
{
    var exception = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
    if (exception is not null)
    {
        logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}.",
            httpContext.Request.Method,
            httpContext.Request.Path);
    }

    return Results.Problem(
        title: "An unexpected error occurred.",
        type: ApiProblemTypes.InternalServerError,
        statusCode: StatusCodes.Status500InternalServerError);
});

app.Run();

public partial class Program;
