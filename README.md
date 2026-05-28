# VitaCare

VitaCare is a learning-focused healthcare management API for patients who require special care. The project is intentionally small, but it is being built with professional structure, clear documentation, and an empathy-driven product mindset.

The first goal is to understand the backend architecture and consolidate a purposeful patient CRUD. The UI/UX layer will be designed later, after the domain and API foundations are clear.

## Project Purpose

VitaCare is not intended to be a generic CRUD exercise. The patient management flow should help represent people with care needs in a respectful, useful, and traceable way.

The project is currently focused on:

- Understanding the structure of a layered .NET backend.
- Modeling patient data with purpose and care context.
- Building a clean API that can later support a thoughtful user interface.
- Keeping the repository organized enough to be presented in a personal portfolio.

This project must not use real patient data.

## Tech Stack

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Swagger / OpenAPI

## Solution Structure

```text
src/
  VitaCare.Api/
    HTTP API entry point, middleware, Swagger, controllers, and API configuration.

  VitaCare.Core/
    Domain entities, enums, and business concepts that should not depend on infrastructure.

  VitaCare.Infrastructure/
    Database access, Entity Framework Core configuration, and persistence services.

scripts/
  verify.sh
    Local verification script used to build and test the solution.
```

## Current Status

The project currently includes:

- A .NET solution with API, Core, and Infrastructure projects.
- Basic domain entities for patients, users, and health records.
- Entity Framework Core configured with PostgreSQL.
- Swagger enabled in development.
- A `/health` endpoint for basic service health checks.

The project does not yet include:

- Patient CRUD endpoints.
- Entity Framework migrations.
- Automated tests.
- A frontend UI.

## Prerequisites

- .NET 9 SDK
- PostgreSQL

## Local Development

Restore and build the solution:

```bash
dotnet build src/VitaCare.sln
```

Run the API:

```bash
dotnet run --project src/VitaCare.Api/VitaCare.Api.csproj
```

Run the verification script:

```bash
scripts/verify.sh
```

When running locally, make sure the `DefaultConnection` connection string points to a PostgreSQL database available in your environment.

## Development Roadmap

1. Clean the repository foundation with documentation and Git hygiene.
2. Refine the patient domain model around special-care needs.
3. Add the patient CRUD API with validation and clear HTTP responses.
4. Add Entity Framework migrations for PostgreSQL.
5. Add focused automated tests.
6. Design and implement the UI/UX layer after the API is stable.

## Commit Style

Use short, structured commit messages based on the type of change:

- `chore:` repository maintenance, tooling, or cleanup.
- `docs:` documentation-only changes.
- `feat:` new user-facing or API functionality.
- `fix:` bug fixes.
- `test:` test coverage changes.
- `refactor:` internal restructuring without behavior changes.
