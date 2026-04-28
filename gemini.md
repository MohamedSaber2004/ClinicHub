# ClinicHub — Global AI Rules

## Project Identity

ClinicHub is a **healthcare clinic management REST API** built with:
- **.NET 10** / **C# 14**
- **ASP.NET Core** (Web API)
- **Clean Architecture** (4 layers: API → Application → Domain → Infrastructure/Persistence)
- **MediatR** (CQRS pattern)
- **FluentValidation**
- **Entity Framework Core** (Persistence layer)
- **Serilog** (structured logging)
- **Scalar + OpenAPI** (API docs)
- **SQL Server** (`CareClinicHubDb` connection string key)

---

## Solution Structure

```
ClinicHub/
├── ClinicHub.API/               # Presentation layer — controllers, filters, Program.cs
├── ClinicHub.Application/       # Business logic — CQRS handlers, validators, services, models
├── ClinicHub.Domain/            # Core domain — entities, interfaces, domain exceptions
├── ClinicHub.Infrastructure/    # External integrations — email, storage, third-party services
└── ClinicHub.Persistence/       # Data access — EF Core DbContext, repositories, migrations
```

**Dependency direction (strict):** API → Application → Domain. Infrastructure and Persistence depend on Application/Domain only. No layer may reference a layer above it.

---

## Architecture Rules

### Clean Architecture Enforcement
- **Domain layer** must have zero dependencies on any other project layer or NuGet package (except pure .NET BCL types).
- **Application layer** must not reference `ClinicHub.API`, `ClinicHub.Infrastructure`, or `ClinicHub.Persistence`.
- **Infrastructure/Persistence layers** implement interfaces defined in the Application or Domain layer; they never expose concrete implementations upward.
- **API layer** is the composition root; all DI wiring happens here via `Program.cs` and the `DependencyInjection` extension methods of each layer.

### CQRS with MediatR
- Every use case must be implemented as a **Command** (write) or **Query** (read) in `ClinicHub.Application`.
- Name convention: `{Entity}{Action}Command` / `{Entity}{Action}Query` and corresponding `{Entity}{Action}CommandHandler` / `{Entity}{Action}QueryHandler`.
- Handlers must be placed under `ClinicHub.Application/{FeatureName}/` with sub-folders `Commands/` and `Queries/`.
- Controllers must never contain business logic — they call MediatR and return an `ApiResponse<T>`.

### Dependency Injection
- Each layer exposes a single `DependencyInjection.cs` static class with an `IServiceCollection` extension method:
  - `AddApplicationServices(IConfiguration)` — Application layer
  - `AddInfrastructureServices()` — Infrastructure layer
  - `AddPersistenceServices()` — Persistence layer
- All new services registered in `Program.cs` must go through these extension methods, not directly in `Program.cs`.

---

## Coding Standards

### Naming Conventions
| Element | Convention | Example |
|---|---|---|
| Classes / Records / Interfaces | PascalCase | `PatientService`, `IPatientRepository` |
| Methods | PascalCase | `GetPatientByIdAsync` |
| Local variables / parameters | camelCase | `patientId`, `cancellationToken` |
| Private fields | `_camelCase` | `_mediator`, `_logger` |
| Constants | PascalCase or UPPER_SNAKE | `MaxPageSize` |
| DTOs / Request models | `{Name}Request` / `{Name}Response` | `CreatePatientRequest` |
| Commands | `{Entity}{Action}Command` | `CreatePatientCommand` |
| Queries | `{Entity}{Action}Query` | `GetPatientByIdQuery` |
| Validators | `{CommandOrQuery}Validator` | `CreatePatientCommandValidator` |

### Language Features
- Always use **C# 14** features where they improve clarity: primary constructors, collection expressions, pattern matching.
- Prefer **records** for DTOs, commands, queries, and responses.
- Use `required` keyword for mandatory record/class properties.
- Always enable **nullable reference types** (`<Nullable>enable</Nullable>` is already set).
- Never suppress nullable warnings with `!` unless absolutely proven safe — add a comment if you do.
- Use `async`/`await` for all I/O-bound operations; never use `.Result` or `.Wait()`.
- Always pass `CancellationToken` through the entire async call chain.

### File Organization
- One top-level type per file.
- File name must match the primary type name exactly.
- Namespace must reflect the folder path (e.g., `ClinicHub.Application.Patients.Commands`).
- Do not use `global using` directives in feature files — centralize them in a `GlobalUsings.cs` per project if needed.

---

## API Layer Rules

### Controllers
- All controllers inherit from `BaseApiController`.
- Use URL-segment API versioning: `[Route("api/v{version:apiVersion}/[controller]")]`.
- Declare supported versions with `[ApiVersion("X.Y")]` attributes.
- Use `[MapToApiVersion("X.Y")]` on individual actions when a controller spans versions.
- Return types must always use the wrapper methods from `BaseApiController`:
  - `Ok<T>(data, message)` for 200
  - `Created<T>(uri, data, message)` for 201
  - `Accepted<T>(data, message)` for 202
  - `Deleted<T>(data, message)` for accepted deletes
- Never return raw `IActionResult` with `new ObjectResult(...)` directly — use the base class helpers.
- Decorate all actions with `[ProducesResponseType(StatusCodes.StatusXXX)]`.
- Add XML `<summary>` doc comments on every public action.

### Exception Handling
- Never use try/catch blocks in controllers or MediatR handlers for domain errors.
- Throw typed exceptions from the Application layer:
  - `NotFoundException` — entity not found
  - `ValidationException` — FluentValidation failures
  - `BadRequestException` — invalid business operation
  - `UnAuthorizedException` — authorization failure
- `ApiExceptionFilterAttribute` handles all of these centrally and maps them to proper HTTP responses.
- For unexpected exceptions, let them bubble up to `ApiExceptionFilterAttribute` which returns 500.
- Always log errors at the appropriate level in `ApiExceptionFilterAttribute` (already implemented for `BadRequestException` and unknowns).

### API Response Format
All responses must use `ApiResponse<TData>`:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "The operation was successful",
  "data": { },
  "errors": {}
}
```
- Never return raw objects directly from controllers.
- Use `ApiResponse<T>.Ok(data, message)` for success.
- Use `ApiResponse<T>.Error(errors, message, statusCode)` for failures (handled by the filter).

---

## Application Layer Rules

### Validation
- Every Command and Query that accepts user input must have a corresponding **FluentValidation** validator.
- Validators are auto-registered via `AddValidatorsFromAssemblies(Assembly.GetExecutingAssembly())`.
- Use `LocalizationKeys.ValidationMessages.*` constants for all validation error messages.
- Example:
  ```csharp
  RuleFor(x => x.Name)
      .NotEmpty().WithMessage(LocalizationKeys.ValidationMessages.Required.Value)
      .MaximumLength(100).WithMessage(LocalizationKeys.ValidationMessages.MaxLength.Value);
  ```

### Localization
- All user-facing strings (messages, errors, validation text) must use the localization system.
- Keys must be defined in `LocalizationKeys` static class using `KeyString` values.
- String resources must be added to **both** `messages.en.json` and `messages.ar.json`.
- Use dot-notation keys matching the JSON structure (e.g., `"Patients.NotFound"`).
- Always use `JsonLocalizationProvider.GetLocalizedString(key)` for static contexts.
- Use `IStringLocalizer<Messages>` / `LocalizationManager` for DI contexts.
- Never hard-code English strings in return values or exception messages visible to API consumers.

### Models
- Use `PagginatedResult<T>` for all paginated list responses (note: intentional double-g spelling in codebase).
- Pagination defaults: `PageNumber = 1`, `PageSize = 20`.
- Use `Result` + `IdentityResultMap` / `SginInResultMap` helpers only for ASP.NET Identity operations.
- Use `ToGuidExtension` helpers (`ToGuid()`, `ToGuid(string?)`, `IsGuid()`) for all Guid parsing — never call `Guid.Parse` directly.

---

## Domain Layer Rules

- Domain entities must be **plain C# classes or records** with no framework dependencies.
- Use **private setters** and **domain methods** to enforce invariants; never expose mutable public setters freely.
- Domain exceptions belong in the Application layer (`ClinicHub.Application.Common.Exceptions`), not the Domain layer, unless they are pure domain invariant violations.
- Define repository interfaces (`IPatientRepository`, etc.) in the Domain or Application layer; implement them in Persistence.

---

## Persistence Layer Rules

- Use **Entity Framework Core** with the `CareClinicHubDb` SQL Server connection string.
- DbContext must be registered in `AddPersistenceServices()`.
- Always use **async EF methods** (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`).
- Always pass `CancellationToken` to EF async calls.
- Use **migrations** for all schema changes: `dotnet ef migrations add <Name> --project ClinicHub.Persistence`.
- Repository implementations must depend on the DbContext only — never on Application services.

---

## Infrastructure Layer Rules

- Register all external service clients and adapters in `AddInfrastructureServices()`.
- Implement interfaces from Application/Domain — never expose infrastructure types to upper layers.
- Use `IOptions<T>` pattern for external service configuration.

---

## Health Checks

The app has three health check components — do not remove or rename them:
1. `CustomHealthCheck` — monitors process memory (threshold: 1 GB).
2. `DatabaseHealthCheck` — verifies SQL Server connectivity via `CareClinicHubDb`.
3. `DiskSpaceHealthCheck` — checks `C:\` drive has ≥ 1024 MB free.

When adding new health checks:
- Implement `IHealthCheck`.
- Register in `AddApplicationServices()` via `services.AddHealthChecks().AddCheck<T>(name)`.
- Singletons that need constructor args (like `DatabaseHealthCheck`) must be registered explicitly with `services.AddSingleton(new T(...))` before the health check registration.

---

## Logging Rules

- Use **Serilog** exclusively — never use `Console.WriteLine` or `Debug.WriteLine` in production code.
- Inject `ILogger<T>` where `T` is the declaring class.
- Use **structured logging** with named properties:
  ```csharp
  _logger.LogInformation("Patient {PatientId} created by {UserId}", patientId, userId);
  ```
- Log levels:
  - `LogDebug` — diagnostic/tracing info (development only)
  - `LogInformation` — normal business events
  - `LogWarning` — recoverable issues or suspicious activity
  - `LogError` — caught exceptions that affect a single operation
  - `LogCritical` / `Log.Fatal` — application-level failures (startup only in `Program.cs`)
- Always log exceptions with the exception object as the first argument: `_logger.LogError(ex, "{Message}", ex.Message)`.
- Never log sensitive data (passwords, tokens, PII).

---

## Configuration Rules

- Connection strings key: `CareClinicHubDb` (used consistently across `DependencyInjection.cs` and health checks).
- Environment-specific files: `appsettings.json` → `appsettings.{Environment}.json` → User Secrets → Environment Variables → CLI args.
- User Secrets are enabled for `Development` and `Test` environments.
- Never commit real credentials to source control — use User Secrets or environment variables.
- Access config via `IConfiguration` in `DependencyInjection` extension methods; use `IOptions<T>` in services.

---

## Versioning Rules

- Default API version: **2.0**.
- `AssumeDefaultVersionWhenUnspecified` is **false** — clients must always specify a version in the URL.
- Version format: `/api/v{version}/[controller]` (URL segment).
- When adding a new version:
  1. Add `[ApiVersion("X.Y")]` to the controller.
  2. Register a new OpenAPI document in `Program.cs` (already done dynamically via `IApiVersionDescriptionProvider`).
  3. Add a new Scalar UI endpoint (also done dynamically).
- Do not remove old versions without a deprecation notice in `[ApiVersion("X.Y", Deprecated = true)]`.

---

## Security Rules

- Always add `[Authorize]` to any controller or action that requires authentication.
- Use `[AllowAnonymous]` explicitly where public access is intentional.
- CORS policy `"CorsPolicy"` is permissive (`AllowAnyOrigin`) — this is intentional for development. For production, restrict origins in `appsettings.Production.json`.
- HTTPS redirection is enabled **only** in Development — ensure production infrastructure enforces HTTPS at the reverse proxy level.
- Never log or return sensitive user data (passwords, tokens, PII) in responses or logs.

---

## Supported Cultures

The API supports two cultures:
- `en` (English) — default
- `ar` (Arabic)

Clients set culture via the `Accept-Language` header or `culture` query parameter. All new localizable strings must have entries in both `messages.en.json` and `messages.ar.json`.

---

## Do's and Don'ts

### ✅ Do
- Follow the existing `BaseApiController` pattern for all new controllers.
- Use `MediatR` for all business operations — keep controllers thin.
- Add FluentValidation validators for every command/query with user input.
- Add localization keys for all new user-facing messages in both language files.
- Write XML doc comments on all public APIs and significant methods.
- Use `async`/`await` and `CancellationToken` consistently.
- Register all services through the layer-specific `DependencyInjection.cs` extension methods.
- Use `ToGuidExtension` for all Guid parsing from strings or objects.
- Follow pagination using `PagginatedResult<T>` with `PageNumber` and `PageSize`.

### ❌ Don't
- Don't put business logic in controllers.
- Don't reference upper layers from lower layers (no circular dependencies).
- Don't use `Task.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` — always await.
- Don't hard-code connection strings, secrets, or environment-specific values in code.
- Don't use `Console.WriteLine` or `Debug.WriteLine` — use Serilog.
- Don't bypass `ApiExceptionFilterAttribute` by wrapping exceptions in try/catch in controllers.
- Don't add new localization keys without updating **both** `messages.en.json` and `messages.ar.json`.
- Don't use `Guid.Parse` directly — use `ToGuidExtension` helpers.
- Don't return raw objects from controllers — always wrap in `ApiResponse<T>`.
- Don't add code directly in `Program.cs` — extend the appropriate `DependencyInjection.cs`.