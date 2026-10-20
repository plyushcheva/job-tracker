# Job Application Tracker

![CI](https://github.com/plyushcheva/job-tracker/actions/workflows/ci.yml/badge.svg)

A full-stack C# / .NET 8 app for tracking job applications: add applications, move them through a status pipeline (Applied → Interview → Offer → Rejected), filter, and see them in a web UI.


## Architecture

```
Blazor WebAssembly UI  ──HTTP/JSON──▶  ASP.NET Core Minimal API  ──EF Core──▶  SQLite
      (JobTracker.Web)                    (JobTracker.Api)
                         shared models: JobTracker.Core  (also used by JobTracker.Cli)
```

| Project | Purpose |
|---|---|
| `JobTracker.Core` | Domain models and a JSON-file store |
| `JobTracker.Api` | REST API, EF Core + SQLite, Swagger documentation |
| `JobTracker.Web` | Blazor WebAssembly frontend |
| `JobTracker.Cli` | Original console version |
| `JobTracker.Tests` | xUnit unit and integration tests |

## Tech

C# 12, .NET 8, ASP.NET Core Minimal APIs, Entity Framework Core, SQLite, Blazor WebAssembly, xUnit, GitHub Actions, Swagger/OpenAPI.

## Run locally

Requires the .NET 8 SDK.

```bash
# terminal 1 - API (http://localhost:5292, docs at /swagger)
cd JobTracker.Api
dotnet run

# terminal 2 - UI (http://localhost:5200)
cd JobTracker.Web
dotnet run --urls http://localhost:5200
```

## Tests

```bash
dotnet test
```


## Known limitations / next steps

- Database created with `EnsureCreated`; next step is EF Core migrations.
- No authentication; single-user, runs locally.
- Possible additions: Docker setup, edit form for existing applications, deployment.
