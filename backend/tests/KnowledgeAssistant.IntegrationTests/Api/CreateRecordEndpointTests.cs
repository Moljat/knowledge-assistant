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
    public async Task GetRecord_WhenRecordExists_ReturnsDetail()
    {
        using var factory = new RecordsApiFactory();
        var client = factory.CreateClient();
        var created = await CreateRecordAsync(client, "Registro detalle");

        var response = await client.GetAsync($"/api/v1/records/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<KnowledgeRecordResponse>();

        Assert.NotNull(body);
        Assert.Equal(created.Id, body.Id);
        Assert.Equal("Registro detalle", body.Title);
    }

    [Fact]
    public async Task GetRecord_WhenRecordDoesNotExist_ReturnsProblemDetails()
    {
        using var factory = new RecordsApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/records/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(body);
        Assert.Equal((int)HttpStatusCode.NotFound, body.Status);
    }

    [Fact]
    public async Task ListRecords_WithPagination_ReturnsRequestedPage()
    {
        using var factory = new RecordsApiFactory();
        var client = factory.CreateClient();
        await CreateRecordAsync(client, "Registro uno");
        await CreateRecordAsync(client, "Registro dos");
        await CreateRecordAsync(client, "Registro tres");

        var response = await client.GetAsync("/api/v1/records?page=2&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PagedKnowledgeRecordResponse>();

        Assert.NotNull(body);
        Assert.Equal(2, body.Page);
        Assert.Equal(2, body.PageSize);
        Assert.Equal(3, body.TotalItems);
        Assert.Equal(2, body.TotalPages);
        Assert.Single(body.Items);
    }

    [Fact]
    public async Task ListRecords_WithInvalidPagination_ReturnsValidationProblem()
    {
        using var factory = new RecordsApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/records?page=0&pageSize=20");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(body);
        Assert.Contains("page", body.Errors.Keys);
    }

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

    [Fact]
    public async Task PutRecord_WithValidPayload_ReturnsUpdatedRecord()
    {
        using var factory = new RecordsApiFactory();
        var client = factory.CreateClient();
        var created = await CreateRecordAsync(client, "Registro editable");
        var request = new UpdateKnowledgeRecordRequest(
            "  Registro actualizado  ",
            "  Contenido actualizado  ",
            "  ERP  ",
            KnowledgeRecordType.BusinessRecord);

        var response = await client.PutAsJsonAsync($"/api/v1/records/{created.Id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<KnowledgeRecordResponse>();

        Assert.NotNull(body);
        Assert.Equal(created.Id, body.Id);
        Assert.Equal("Registro actualizado", body.Title);
        Assert.Equal("Contenido actualizado", body.Content);
        Assert.Equal("ERP", body.Source);
        Assert.Equal(KnowledgeRecordType.BusinessRecord, body.Type);
    }

    [Fact]
    public async Task PutRecord_WhenRecordDoesNotExist_ReturnsProblemDetails()
    {
        using var factory = new RecordsApiFactory();
        var client = factory.CreateClient();
        var request = new UpdateKnowledgeRecordRequest(
            "Registro",
            "Contenido",
            null,
            KnowledgeRecordType.Note);

        var response = await client.PutAsJsonAsync($"/api/v1/records/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(body);
        Assert.Equal((int)HttpStatusCode.NotFound, body.Status);
    }

    [Fact]
    public async Task PutRecord_WithInvalidPayload_ReturnsValidationProblem()
    {
        using var factory = new RecordsApiFactory();
        var client = factory.CreateClient();
        var created = await CreateRecordAsync(client, "Registro invalido");
        var request = new UpdateKnowledgeRecordRequest(
            " ",
            "Contenido",
            null,
            KnowledgeRecordType.Note);

        var response = await client.PutAsJsonAsync($"/api/v1/records/{created.Id}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(body);
        Assert.Contains("title", body.Errors.Keys);
    }

    [Fact]
    public async Task DeleteRecord_WhenRecordExists_ReturnsNoContentAndRemovesRecord()
    {
        using var factory = new RecordsApiFactory();
        var client = factory.CreateClient();
        var created = await CreateRecordAsync(client, "Registro eliminable");

        var response = await client.DeleteAsync($"/api/v1/records/{created.Id}");
        var getAfterDelete = await client.GetAsync($"/api/v1/records/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task DeleteRecord_WhenRecordDoesNotExist_ReturnsProblemDetails()
    {
        using var factory = new RecordsApiFactory();
        var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/v1/records/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(body);
        Assert.Equal((int)HttpStatusCode.NotFound, body.Status);
    }

    private static async Task<KnowledgeRecordResponse> CreateRecordAsync(
        HttpClient client,
        string title)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/records",
            new CreateKnowledgeRecordRequest(
                title,
                "Contenido",
                "Pruebas",
                KnowledgeRecordType.Note));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<KnowledgeRecordResponse>();

        return body!;
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

        public Task<PagedKnowledgeRecordResult> ListAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var orderedRecords = _records.Values
                .OrderByDescending(record => record.CreatedAtUtc)
                .ThenBy(record => record.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(new PagedKnowledgeRecordResult(
                orderedRecords,
                page,
                pageSize,
                _records.Count));
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
