# VitaCare — Patient Care Management API

A learning-focused healthcare platform that helps represent and manage patients who require special care — modeling patient data with purpose and traceability, exposed through a clean API and a lightweight, empathy-driven interface.

> This project must not use real patient data.

## Tech Stack

| Layer | Technology |
| --- | --- |
| Frontend | HTML + CSS + Vanilla JavaScript (static UI served from `wwwroot`) |
| Backend | .NET 9 + ASP.NET Core Web API (layered architecture) |
| Data access | Entity Framework Core (Npgsql provider) |
| Database | PostgreSQL |
| API docs | Swagger / OpenAPI (Swashbuckle) |
| Testing | xUnit + EF Core InMemory |

## Architecture

A layered .NET backend with explicit boundaries between the HTTP surface, the domain, and infrastructure, so the domain never depends on persistence details:

- **VitaCare.Api** — HTTP entry point, middleware, Swagger, controllers, and the static patient UI (`wwwroot`).
- **VitaCare.Core** — domain entities, enums, and business concepts, free of infrastructure dependencies.
- **VitaCare.Infrastructure** — database access, Entity Framework Core configuration, and persistence services.

See `docs/architecture.md` for the full plan.

## Getting Started

Make sure the `DefaultConnection` connection string points to a PostgreSQL database available in your environment.

```bash
dotnet build src/VitaCare.sln                          # restore and build
dotnet run --project src/VitaCare.Api/VitaCare.Api.csproj   # start the API + UI
scripts/verify.sh                                      # build and run the tests
```

Once running, the patient UI is served at the API root and the OpenAPI docs at `/swagger`.

## Project Conventions

- **Structure:** layered solution (`Api`, `Core`, `Infrastructure`) with the domain isolated from infrastructure.
- **Commits:** short, type-prefixed messages (`feat:`, `fix:`, `chore:`, `docs:`, `refactor:`, `test:`).
- **Branching:** stable `main` with focused feature branches merged via PR.
- **Language:** all code, comments, and commit messages in English.
- **Data:** no real patient data — the model is for learning and portfolio purposes only.
