using System.Net;
using System.Net.Http.Json;
using KnowledgeAssistant.Api.Controllers;
using KnowledgeAssistant.Application.Abstractions;
using KnowledgeAssistant.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KnowledgeAssistant.IntegrationTests.Api;

public sealed class CreateRecordEndpointTests
{
    [Fact]
    public async Task PostRecord_WithValidPayload_ReturnsCreatedRecord()
    {
        using var factory = new RecordsApiFactory();
        var client = factory.CreateClient();
        var request = new CreateKnowledgeRecordRequest(
            "  Politica de credito  ",
            "  Registrar aprobaciones y excepciones.  ",
            "  Manual interno  ",
            KnowledgeRecordType.Document);

        var response = await client.PostAsJsonAsync("/api/v1/records", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var body = await response.Content.ReadFromJsonAsync<KnowledgeRecordResponse>();

        Assert.NotNull(body);
        Assert.Equal("Politica de credito", body.Title);
        Assert.Equal("Registrar aprobaciones y excepciones.", body.Content);
        Assert.Equal("Manual interno", body.Source);
        Assert.Equal(KnowledgeRecordType.Document, body.Type);
        Assert.Equal(KnowledgeRecordStatus.Draft, body.Status);
        Assert.Equal(AiProcessingStatus.NotRequested, body.AiStatus);
        Assert.EndsWith($"/api/v1/records/{body.Id}", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task PostRecord_WithInvalidPayload_ReturnsValidationProblem()
    {
        using var factory = new RecordsApiFactory();
        var client = factory.CreateClient();
        var request = new CreateKnowledgeRecordRequest(
            " ",
            "Contenido",
            null,
            KnowledgeRecordType.Note);

        var response = await client.PostAsJsonAsync("/api/v1/records", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(body);
        Assert.Contains("title", body.Errors.Keys);
    }

    private sealed class RecordsApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IKnowledgeRecordRepository>();
                services.RemoveAll<IUnitOfWork>();
                services.AddSingleton<IKnowledgeRecordRepository, InMemoryRepository>();
                services.AddSingleton<IUnitOfWork, SuccessfulUnitOfWork>();
            });
        }
    }

    private sealed class InMemoryRepository : IKnowledgeRecordRepository
    {
        private readonly Dictionary<Guid, KnowledgeRecord> _records = [];

        public Task<KnowledgeRecord?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _records.TryGetValue(id, out var record);
            return Task.FromResult(record);
        }

        public Task<KnowledgeRecord?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _records.TryGetValue(id, out var record);
            return Task.FromResult(record);
        }

        public Task<bool> ExistsAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_records.ContainsKey(id));
        }

        public void Add(KnowledgeRecord record)
        {
            _records.Add(record.Id, record);
        }

        public void Remove(KnowledgeRecord record)
        {
            _records.Remove(record.Id);
        }
    }

    private sealed class SuccessfulUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }
}
