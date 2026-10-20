using System.Text.Json.Serialization;
using JobTracker.Models;
using JobTracker.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var dataFile = builder.Configuration["DataFile"] ?? "applications.json";
builder.Services.AddSingleton(new ApplicationStore(dataFile));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var api = app.MapGroup("/api");

api.MapGet("/applications", (ApplicationStore store, ApplicationStatus? status) =>
    status is null ? store.GetAll() : store.FilterByStatus(status.Value));

api.MapGet("/applications/{id:int}", (int id, ApplicationStore store) =>
    store.GetById(id) is { } application ? Results.Ok(application) : Results.NotFound());

api.MapPost("/applications", (CreateApplicationRequest request, ApplicationStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.Company) || string.IsNullOrWhiteSpace(request.Position))
        return Results.BadRequest("Company and Position are required.");

    var created = store.Add(request.Company.Trim(), request.Position.Trim(), request.AppliedOn, request.Notes);
    return Results.Created($"/api/applications/{created.Id}", created);
});

api.MapPut("/applications/{id:int}/status", (int id, UpdateStatusRequest request, ApplicationStore store) =>
    store.UpdateStatus(id, request.Status) ? Results.NoContent() : Results.NotFound());

api.MapDelete("/applications/{id:int}", (int id, ApplicationStore store) =>
    store.Delete(id) ? Results.NoContent() : Results.NotFound());

api.MapGet("/stats", (ApplicationStore store) => store.CountByStatus());

app.Run();

public record CreateApplicationRequest(string Company, string Position, DateOnly? AppliedOn, string? Notes);
public record UpdateStatusRequest(ApplicationStatus Status);