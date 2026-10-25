using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using JobTracker.Api.Data;
using JobTracker.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            services.Remove(descriptor);
            services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={_dbPath}"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var suffix in new[] { "", "-wal", "-shm" })
            File.Delete(_dbPath + suffix);
    }
}

public class ApiTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public ApiTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Post_creates_application_and_returns_201()
    {
        var response = await _client.PostAsJsonAsync("/api/applications", new { company = "Test Corp", position = "Engineer" }, Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<JobApplication>(Json);
        Assert.NotNull(created);
        Assert.Equal("Test Corp", created.Company);
        Assert.Equal(ApplicationStatus.Applied, created.Status);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), created.AppliedOn);
    }

    [Fact]
    public async Task Post_with_empty_company_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/applications", new { company = "", position = "Engineer" }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_unknown_id_returns_404()
    {
        var response = await _client.GetAsync("/api/applications/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Status_can_be_updated_and_filtered()
    {
        var created = await CreateAsync("Filter Corp", "Developer");

        var put = await _client.PutAsJsonAsync($"/api/applications/{created.Id}/status", new { status = ApplicationStatus.Interview }, Json);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var interviews = await _client.GetFromJsonAsync<List<JobApplication>>("/api/applications?status=Interview", Json);
        Assert.Contains(interviews!, a => a.Id == created.Id);
    }

    [Fact]
    public async Task Delete_removes_application()
    {
        var created = await CreateAsync("Delete Corp", "Developer");

        var delete = await _client.DeleteAsync($"/api/applications/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await _client.GetAsync($"/api/applications/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    private async Task<JobApplication> CreateAsync(string company, string position)
    {
        var response = await _client.PostAsJsonAsync("/api/applications", new { company, position }, Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JobApplication>(Json))!;
    }
    [Fact]
public async Task Put_updates_application_fields()
{
    var created = await CreateAsync("Typo Corp", "Developer");

    var response = await _client.PutAsJsonAsync($"/api/applications/{created.Id}",
        new { company = "Fixed Corp", position = "Backend Developer", appliedOn = new DateOnly(2026, 10, 1), notes = "corrected" }, Json);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var updated = await _client.GetFromJsonAsync<JobApplication>($"/api/applications/{created.Id}", Json);
    Assert.Equal("Fixed Corp", updated!.Company);
    Assert.Equal("Backend Developer", updated.Position);
    Assert.Equal(new DateOnly(2026, 10, 1), updated.AppliedOn);
    Assert.Equal(ApplicationStatus.Applied, updated.Status);
}

[Fact]
public async Task Put_with_unknown_id_returns_404()
{
    var response = await _client.PutAsJsonAsync("/api/applications/999999",
        new { company = "X", position = "Y", appliedOn = new DateOnly(2026, 10, 1), notes = (string?)null }, Json);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
    
}