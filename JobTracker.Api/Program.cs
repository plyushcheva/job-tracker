using System.Text.Json.Serialization;
using JobTracker.Api.Data;
using JobTracker.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:5200").AllowAnyHeader().AllowAnyMethod()));

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=applications.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

var api = app.MapGroup("/api");

api.MapGet("/applications", async (AppDbContext db, ApplicationStatus? status) =>
{
    var query = db.Applications.AsNoTracking();
    if (status is not null)
        query = query.Where(a => a.Status == status);

    return await query.OrderByDescending(a => a.AppliedOn).ThenBy(a => a.Id).ToListAsync();
});

api.MapGet("/applications/{id:int}", async (int id, AppDbContext db) =>
    await db.Applications.FindAsync(id) is { } application ? Results.Ok(application) : Results.NotFound());

api.MapPost("/applications", async (CreateApplicationRequest request, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Company) || string.IsNullOrWhiteSpace(request.Position))
        return Results.BadRequest("Company and Position are required.");

    var application = new JobApplication
    {
        Company = request.Company.Trim(),
        Position = request.Position.Trim(),
        AppliedOn = request.AppliedOn ?? DateOnly.FromDateTime(DateTime.Today),
        Notes = request.Notes
    };

    db.Applications.Add(application);
    await db.SaveChangesAsync();
    return Results.Created($"/api/applications/{application.Id}", application);
});

api.MapPut("/applications/{id:int}", async (int id, UpdateApplicationRequest request, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Company) || string.IsNullOrWhiteSpace(request.Position))
        return Results.BadRequest("Company and Position are required.");

    var application = await db.Applications.FindAsync(id);
    if (application is null) return Results.NotFound();

    application.Company = request.Company.Trim();
    application.Position = request.Position.Trim();
    application.AppliedOn = request.AppliedOn;
    application.Notes = request.Notes;
    await db.SaveChangesAsync();
    return Results.Ok(application);
});

api.MapPut("/applications/{id:int}/status", async (int id, UpdateStatusRequest request, AppDbContext db) =>
{
    var application = await db.Applications.FindAsync(id);
    if (application is null) return Results.NotFound();

    application.Status = request.Status;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapDelete("/applications/{id:int}", async (int id, AppDbContext db) =>
{
    var application = await db.Applications.FindAsync(id);
    if (application is null) return Results.NotFound();

    db.Applications.Remove(application);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapGet("/stats", async (AppDbContext db) =>
    await db.Applications
        .GroupBy(a => a.Status)
        .Select(group => new { Status = group.Key, Count = group.Count() })
        .ToListAsync());

app.Run();

public record CreateApplicationRequest(string Company, string Position, DateOnly? AppliedOn, string? Notes);
public record UpdateStatusRequest(ApplicationStatus Status);

public record UpdateApplicationRequest(string Company, string Position, DateOnly AppliedOn, string? Notes);

public partial class Program { }