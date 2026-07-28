# ClinicHub — AGENTS.md

## Quick start

```powershell
dotnet restore ClinicHub.slnx
dotnet build ClinicHub.slnx
dotnet run --project ClinicHub.API            # Development (http)
dotnet run --project ClinicHub.API --launch-profile "https"
dotnet run --project ClinicHub.API --launch-profile "ClinicHub.API"   # Test env
```

## Solution structure (Clean Architecture, CQRS)

```
Domain  ←  Application  ←  Persistence  →  Infrastructure  →  API
```

- **Domain** — entities, enums, repository interfaces, no project deps
- **Application** — MediatR commands/queries, DTOs, AutoMapper profiles, FluentValidation validators, localizer, interfaces for infra services. Depends only on Domain.
- **Persistence** — EF Core `DbContext`, configurations, migrations, seeders (Bogus). Depends on Application.
- **Infrastructure** — repository implementations (GenericRepository + UnitOfWork), external services (Paymob, Pusher, Email, JWT, Google/Facebook auth, Maps). Depends on Application + Persistence.
- **API** — controllers, middleware, filters, routes. Depends on all lower layers.

DI registration files: `ClinicHub.*/DependencyInjection.cs` (4 files). Called in order from Program.cs: `AddApplicationServices` → `AddPersistenceServices` → `AddInfrastructureServices`.

## .agents (built-in instruction files)

- `.agents/rules/clinichubrole.md` — coding conventions, always-on
- `.agents/workflows/clinichubworkflow.md` — 12-phase feature implementation workflow
- `.agents/prompts/extract-last-message.prompt.md` — conversation analysis prompt

## Key conventions

- **CQRS with MediatR**: Commands in `Features/*/Commands/`, Queries in `Features/*/Queries/`. Handlers named `*CommandHandler` / `*QueryHandler`. Pipeline behaviours in order: `UnhandledException` → `Validation` → `Performance` (>600ms threshold) → `Logging`.
- **User-facing messages** use `IStringLocalizer<Messages>` with JSON files at `ClinicHub.Application/Localization/Resources/messages.{en,ar}.json`. Default culture is Arabic. Set via `Accept-Language` header.
- **Soft delete**: `BaseEntity.IsDeleted` — `DbContext.SaveChangesAsync` intercepts `EntityState.Deleted`. Check `IsDeleted` in queries.
- **API responses**: Always return `ApiResponse<T>` via helper methods on `BaseApiController` (`Ok()`, `Created()`, `Deleted()`, `Accepted()`). Global exception handling via `ApiExceptionFilterAttribute`.
- **Controller pattern**: `BaseApiController` injects `IMediator`. Route constants in `Routes/ApiRoutes.cs`. Controllers are `[ApiVersion("1.0")]`, most require `[Authorize]` (exceptions: auth endpoints).
- **API docs** at `/scalar/{versionName}` (not Swagger). Root `/` redirects to latest version. Two OpenAPI docs: v1 and v2.

## Persistence / EF Core

- SQL Server + NetTopologySuite (spatial types for clinic geo-search)
- Migrations in `ClinicHub.Persistence/Migrations/`
- Seed data runs 5s after startup via `Task.Run` (roles, specializations from `SeedData/specializations.json`, test data with Bogus)
- NET-Tracker tables created manually at startup via `IRelationalDatabaseCreator.CreateTablesAsync()`

```powershell
dotnet ef migrations add <Name> --project ClinicHub.Persistence --startup-project ClinicHub.API
dotnet ef database update --project ClinicHub.Persistence --startup-project ClinicHub.API
dotnet ef migrations list --project ClinicHub.Persistence --startup-project ClinicHub.API
```

## Project Skills

- **`clinichub-execution`** (`.agents/skills/clinichub-execution/SKILL.md`) — Execute code systematically following Clean Architecture layers, CQRS patterns, and project conventions. Use when implementing features, writing code, or executing plans.
- **`clinichub-sync`** (`.agents/skills/clinichub-sync/SKILL.md`) — Scan the project for new entities, features, or pattern changes and auto-update `clinichub-execution` to keep it in sync with the current codebase.

## Notable

- `appsettings.json` is gitignored and stores `PaymobSettings` template — copy before editing
- `global.json` pins SDK `10.0.201` (rollForward: latestMinor)
- No test projects exist
- `.github/workflows/` is empty — no CI configured
- .NET 10, `net10.0`, `Nullable` enable, `ImplicitUsings` enable
- Solution uses `.slnx` format (VS 2022+)
- Rate limiting via `AspNetCoreRateLimit` (in-memory, IP-based in `appsettings.json`)
- Tracking/monitoring via `NET-Tracker` (auto-creates tables at startup, dashboard at `/net-tracker/dashboard`)
- Custom file serving at `/files` via `CustomFileProvider`
