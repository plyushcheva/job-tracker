using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using JobTracker.Models;

namespace JobTracker.Web.Services;

public class ApplicationsClient(HttpClient http)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<List<JobApplication>> GetAllAsync(ApplicationStatus? status = null)
    {
        var url = status is null ? "api/applications" : $"api/applications?status={status}";
        return await http.GetFromJsonAsync<List<JobApplication>>(url, Json) ?? new();
    }

    public async Task<bool> AddAsync(string company, string position, DateOnly appliedOn, string? notes)
    {
        var response = await http.PostAsJsonAsync("api/applications", new { company, position, appliedOn, notes }, Json);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateStatusAsync(int id, ApplicationStatus status)
    {
        var response = await http.PutAsJsonAsync($"api/applications/{id}/status", new { status }, Json);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"api/applications/{id}");
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> UpdateAsync(int id, string company, string position, DateOnly appliedOn, string? notes)
    {
        var response = await http.PutAsJsonAsync($"api/applications/{id}", new { company, position, appliedOn, notes }, Json);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<StatusCount>> GetStatsAsync() =>
        await http.GetFromJsonAsync<List<StatusCount>>("api/stats", Json) ?? new();
}

    public record StatusCount(ApplicationStatus Status, int Count);