# ClinicHub — Project Rules & Implementation Guidelines
# For use with Cursor, Windsurf, GitHub Copilot, or any AI coding assistant.
# ============================================================
# Every suggestion, generation, or edit MUST comply with ALL rules below.
# ============================================================

## ──────────────────────────────────────────────
## 1. TECHNOLOGY STACK (HARD CONSTRAINTS)
## ──────────────────────────────────────────────

- Runtime        : .NET 10 (SDK 10.0.201, rollForward latestMinor)
- Language       : C# 14 — enable nullable, enable implicit usings
- Web framework  : ASP.NET Core (Microsoft.NET.Sdk.Web)
- ORM            : Entity Framework Core 10 (SQL Server provider)
- Mediator       : MediatR 14
- Validation     : FluentValidation 12 (DI extensions)
- API docs       : Microsoft.AspNetCore.OpenApi 10 + Scalar.AspNetCore 2
- API versioning : Asp.Versioning.Http 8 + Asp.Versioning.Mvc.ApiExplorer 8
- Logging        : Serilog.AspNetCore 10 (Console + File sinks)
- Health checks  : AspNetCore.HealthChecks.SqlServer 9 + System 9

NEVER downgrade, replace, or add conflicting packages without explicit approval.

## ──────────────────────────────────────────────
## 2. SOLUTION STRUCTURE (CLEAN ARCHITECTURE)
## ──────────────────────────────────────────────

```
ClinicHub/
├── ClinicHub.API/              ← Presentation (HTTP, Controllers, Filters, Services)
├── ClinicHub.Application/      ← Business Logic (MediatR Handlers, Validators, Interfaces)
├── ClinicHub.Domain/           ← Core (Entities, Enums, Domain Interfaces, Repo Interfaces)
├── ClinicHub.Infrastructure/   ← Infrastructure (Repo Implementations, External services)
└── ClinicHub.Persistence/      ← Data (EF DbContext, Migrations, EF Configurations)
```

### Dependency direction (never violate):
```
API  →  Application  →  Domain
         ↑                ↑
   Infrastructure   Persistence
```
- API may reference Application, Infrastructure, Persistence.
- Application references Domain ONLY.
- Infrastructure references Application + Persistence.
- Persistence references Application.
- Domain has ZERO external references (no NuGet, no other projects).

## ──────────────────────────────────────────────
## 3. NAMING CONVENTIONS
## ──────────────────────────────────────────────

| Artifact                  | Convention                        | Example                        |
|---------------------------|-----------------------------------|--------------------------------|
| Classes / Records         | PascalCase                        | `PatientService`               |
| Interfaces                | I + PascalCase                    | `IPatientService`              |
| Methods                   | PascalCase                        | `GetByIdAsync`                 |
| Private fields            | _camelCase                        | `_mediator`                    |
| Local variables / params  | camelCase                         | `patientId`                    |
| Constants                 | ALL_CAPS or PascalCase            | `MaxPageSize`                  |
| Async methods             | Suffix `Async`                    | `CreatePatientAsync`           |
| DTOs / Queries / Commands | Descriptive + suffix              | `CreatePatientCommand`         |
| Validators                | EntityName + Validator            | `CreatePatientCommandValidator`|
| EF Configurations         | EntityName + Configuration        | `PatientConfiguration`         |
| Controller names          | EntityName + Controller           | `PatientsController`           |
| Repository interfaces     | I + EntityName + Repository       | `IPatientRepository`           |

## ──────────────────────────────────────────────
## 4. DOMAIN LAYER RULES (ClinicHub.Domain)
## ──────────────────────────────────────────────

### 4.1 BaseEntity
All domain entities MUST inherit from:
```csharp
public class BaseEntity<TKey> : IBaseEntity<TKey>
```
- TKey is always `Guid` unless there is an explicit requirement for a different type.
- Constructor must call `base()` which auto-sets `Id = Guid.NewGuid()`, `IsActive = true`, `IsDeleted = false`.
- Fields: Id, CreatedAt, UpdatedAt, DeletedAt, CreatedBy, UpdatedBy, DeletedBy, IsDeleted, IsActive.

### 4.2 Entity rules
- Entities live in `ClinicHub.Domain/Entities/`.
- Enums live in `ClinicHub.Domain/Enums/`.
- Entities are plain C# classes — no EF Core attributes (data annotations) except `[Key]` on IBaseEntity.
- All entity configuration is done in EF Fluent API (ClinicHub.Persistence/Configuration/).
- Navigation properties use `virtual` only when lazy loading is explicitly required.

### 4.3 Soft delete convention
- NEVER use hard delete (`context.Remove()`) on production entities.
- Soft delete = set `IsDeleted = true` + `DeletedAt` + `DeletedBy` via `SaveChangesAsync` override.
- All queries MUST filter `IsDeleted == false` by default via Global Query Filters.

### 4.4 Repository interfaces
- Defined in `ClinicHub.Domain/Repositories/Interfaces/`.
- Always extend `IGenericRepository<T, TKey>`.
- Entity-specific methods go in a dedicated interface:
  ```csharp
  public interface IPatientRepository : IGenericRepository<Patient, Guid>
  {
      Task<Patient?> GetByNationalIdAsync(string nationalId, CancellationToken ct = default);
  }
  ```

## ──────────────────────────────────────────────
## 5. APPLICATION LAYER RULES (ClinicHub.Application)
## ──────────────────────────────────────────────

### 5.1 MediatR — CQRS Pattern
Every feature MUST be implemented as a Command or Query.

**Query (read-only):**
```csharp
// ClinicHub.Application/Features/Patients/Queries/GetPatients/
public record GetPatientsQuery(int PageNumber, int PageSize) : IRequest<PagginatedResult<PatientDto>>;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, PagginatedResult<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetPatientsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagginatedResult<PatientDto>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        // implementation
    }
}
```

**Command (write):**
```csharp
// ClinicHub.Application/Features/Patients/Commands/CreatePatient/
public record CreatePatientCommand(string FirstName, string LastName, string NationalId) : IRequest<Guid>;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Guid>
{
    // ...
}
```

### 5.2 FluentValidation
- Every Command and Query that accepts user input MUST have a corresponding validator.
- Validator class lives in the SAME folder as its Command/Query.
- Use `IRequest` pipeline behavior for automatic validation (register `ValidationBehavior<,>`).
- NEVER throw raw exceptions from validators — let FluentValidation produce `ValidationException`.

```csharp
public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(LocalizationKeys.ValidationMessages.Required.Value)
            .MaximumLength(100).WithMessage(LocalizationKeys.ValidationMessages.MaxLength.Value);

        RuleFor(x => x.NationalId)
            .NotEmpty()
            .Length(14).WithMessage("National ID must be 14 digits");
    }
}
```

### 5.3 Folder structure per feature
```
ClinicHub.Application/Features/{Entity}/
├── Commands/
│   ├── Create{Entity}/
│   │   ├── Create{Entity}Command.cs
│   │   ├── Create{Entity}CommandHandler.cs
│   │   └── Create{Entity}CommandValidator.cs
│   ├── Update{Entity}/
│   └── Delete{Entity}/
├── Queries/
│   ├── Get{Entity}ById/
│   └── Get{Entities}/
└── DTOs/
    └── {Entity}Dto.cs
```

### 5.4 DTOs
- DTOs are records (immutable by default).
- Never expose domain entities directly from handlers — always map to DTOs.
- Use manual mapping or create extension methods in `Mappers/`.

### 5.5 Interfaces
- All infrastructure contracts live in `ClinicHub.Application/Common/Interfaces/`.
- Example: `IClinicHubContext`, `ICurrentUserService`, `IEmailService`.

### 5.6 ApiResponse<T> — ALWAYS use the wrapper
All handler return types used by controllers must ultimately be wrapped in `ApiResponse<T>` at the controller level:
```csharp
return Ok(result);          // wraps in ApiResponse<T>.Ok(...)
return Created(uri, result); // wraps in ApiResponse<T>.Ok(...) with 201
```
NEVER return raw data directly from controllers.

### 5.7 Pagination
Always use `PagginatedResult<T>` for list endpoints.
Queries must accept `int PageNumber` (default 1) and `int PageSize` (default 20, max 100).

### 5.8 Localization
- All user-facing strings MUST use `LocalizationKeys.*`.
- Localization keys map to `messages.en.json` and `messages.ar.json`.
- Adding a new message: add the key to `LocalizationKeys.cs` AND both JSON files.
- Format: dot-notation matching JSON nesting — e.g., `"ActionResults.Ok"`.

## ──────────────────────────────────────────────
## 6. PERSISTENCE LAYER RULES (ClinicHub.Persistence)
## ──────────────────────────────────────────────

### 6.1 DbContext — ClinicHubContext
- Always use the existing `ClinicHubContext` — do NOT create additional DbContext classes.
- `SaveChangesAsync` override auto-populates audit fields (CreatedAt, UpdatedAt, etc.) via `ICurrentUserService`.
- All entity registrations use `builder.ApplyConfigurationsFromAssembly(...)` — no manual `DbSet<>` mapping for each entity.

### 6.2 EF Configuration
```csharp
// ClinicHub.Persistence/Configuration/PatientConfiguration.cs
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.HasQueryFilter(p => !p.IsDeleted); // MANDATORY soft-delete filter
    }
}
```
- Namespace MUST end with `Configuration` (enforced by `ApplyConfigurationsFromAssembly` filter).
- Every entity MUST have `HasQueryFilter(e => !e.IsDeleted)`.
- Use `HasMaxLength` on all string properties — never leave unlimited strings.

### 6.3 Migrations
```bash
# Always specify the project
dotnet ef migrations add <MigrationName> --project ClinicHub.Persistence --startup-project ClinicHub.API
dotnet ef database update --project ClinicHub.Persistence --startup-project ClinicHub.API
```
- Migration names use PascalCase and describe the change: `AddPatientTable`, `AddAppointmentStatusColumn`.
- Never edit generated migration files manually — use `dotnet ef migrations remove` and recreate.

### 6.4 Connection string key
Always use: `"CareClinicHubDb"` — never introduce a different key name.

## ──────────────────────────────────────────────
## 7. INFRASTRUCTURE LAYER RULES (ClinicHub.Infrastructure)
## ──────────────────────────────────────────────

### 7.1 Repository implementations
- Implement `IGenericRepository<T, TKey>` via `GenericRepository<T, TKey>`.
- Entity-specific repositories extend `GenericRepository<T, Guid>` and implement their interface.
- ALL public `IGenericRepository` methods must be fully implemented — no `throw new NotImplementedException()` in production code.

Full implementation template:
```csharp
public IQueryable<T> GetAllAsync(Expression<Func<T, bool>>? predicate)
{
    var query = _context.Set<T>().AsQueryable();
    return predicate != null ? query.Where(predicate) : query;
}

public async Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    => await _context.Set<T>().FirstOrDefaultAsync(predicate, ct);

public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    => await _context.Set<T>().AnyAsync(predicate, ct);
```

### 7.2 Unit of Work
- Use `IUnitOfWork` for ALL database write operations in handlers.
- Pattern:
```csharp
await _uow.BeginTransactionAsync();
try {
    var repo = _uow.GetRepository<Patient, Guid>();
    await repo.AddAsync(entity);
    await _uow.SaveChangesAsync();
    await _uow.CommitAsync();
} catch {
    await _uow.RollbackAsync();
    throw;
}
```
- `IUnitOfWork` is registered as **Scoped**.

### 7.3 Registering services
All registrations go in `ClinicHub.Infrastructure/DependencyInjection.cs`:
```csharp
public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
{
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
    services.AddScoped<ICurrentUserService, CurrentUserService>();
    // entity-specific repos ...
    return services;
}
```

## ──────────────────────────────────────────────
## 8. API LAYER RULES (ClinicHub.API)
## ──────────────────────────────────────────────

### 8.1 Controller structure
```csharp
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class PatientsController : BaseApiController
{
    public PatientsController(IMediator mediator) : base(mediator) { }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPatientByIdQuery(id), ct);
        return Ok(result);
    }
}
```

### 8.2 BaseApiController rules
- ALL controllers extend `BaseApiController`.
- NEVER use `base.Ok(...)` directly — always use the overloaded `Ok(data)` / `Ok(message)` / `Created(uri, data)` / `Deleted(data)` helpers.
- NEVER return raw objects — always return `IActionResult`.
- NEVER inject services other than `IMediator` into controllers — all business logic goes through MediatR.

### 8.3 API Versioning
- Default version: **2.0**.
- Reader strategy: **URL segment** (`/api/v1/`, `/api/v2/`).
- New endpoints default to the current highest version.
- Breaking changes → bump major version; non-breaking additions → add to existing version.
- Always add `[MapToApiVersion("X.Y")]` explicitly.

### 8.4 Exception handling
- NEVER use try/catch in controllers.
- ALL exceptions are handled by `ApiExceptionFilterAttribute`.
- Throw domain-specific exceptions from handlers:
  - `NotFoundException` → 404
  - `ValidationException` → 400
  - `BadRequestException` → 400
  - `UnAuthorizedException` → 401
  - Unhandled → 500

### 8.5 ProducesResponseType attributes
Every action MUST declare all possible response types:
```csharp
[ProducesResponseType(typeof(ApiResponse<PatientDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
```

### 8.6 CORS
- Policy name: `"CorsPolicy"` — do not change or add new policies without approval.

### 8.7 CurrentUserService
- Inject `ICurrentUserService` only in handlers that need user context — NOT in controllers.
- `UserId` returns `Guid.Empty` for unauthenticated requests — always check `IsAuthenticated` first.

## ──────────────────────────────────────────────
## 9. HEALTH CHECKS
## ──────────────────────────────────────────────

- Endpoint: `/health`
- Three checks must always pass: `CustomHealthCheck` (memory), `DiskSpaceHealthCheck` (≥1 GB free), `DatabaseHealthCheck`.
- Adding a new health check: implement `IHealthCheck`, register in `Application/DependencyInjection.cs`.
- Response format is handled by `HealthCheckResponseWriter.WriteResponse` — do NOT change the response writer.

## ──────────────────────────────────────────────
## 10. LOGGING RULES
## ──────────────────────────────────────────────

- Use **Serilog** — never use `ILogger<T>` from Microsoft.Extensions.Logging directly in new code (use Serilog's structured logging).
- Inject `ILogger<T>` only where Serilog is not available (e.g., filters already have it injected).
- Log levels:
  - `LogInformation` — normal operations, request lifecycle.
  - `LogWarning` — unexpected but recoverable situations.
  - `LogError(exception, message)` — caught exceptions, bad requests with stack trace.
  - `LogFatal` — only in `Program.cs` top-level catch.
- NEVER log sensitive data: passwords, tokens, connection strings, personal health info (PHI).
- Always include structured properties:
  ```csharp
  _logger.LogInformation("Patient {PatientId} created by {UserId}", patient.Id, userId);
  ```

## ──────────────────────────────────────────────
## 11. LOCALIZATION RULES
## ──────────────────────────────────────────────

- Supported cultures: `en` (default), `ar`.
- Culture is determined by `Accept-Language` header.
- JSON files: `Localization/Resources/messages.en.json` and `messages.ar.json`.
- Key format: `Section.SubSection.KeyName` (dot-notation).
- NEVER hardcode user-facing strings in C# — always reference `LocalizationKeys.*`.

Adding a new localization key (REQUIRED steps):
1. Add key class in `LocalizationKeys.cs`:
   ```csharp
   public static class PatientMessages
   {
       public static readonly KeyString NotFound = new("Patients.NotFound");
       public static readonly KeyString Created   = new("Patients.Created");
   }
   ```
2. Add English value in `messages.en.json`:
   ```json
   { "Patients": { "NotFound": "Patient not found", "Created": "Patient created successfully" } }
   ```
3. Add Arabic value in `messages.ar.json`:
   ```json
   { "Patients": { "NotFound": "المريض غير موجود", "Created": "تم إنشاء المريض بنجاح" } }
   ```

## ──────────────────────────────────────────────
## 12. SECURITY RULES
## ──────────────────────────────────────────────

- HTTPS is enforced in Development only (see `Program.cs`) — Production terminates TLS at the reverse proxy.
- Authentication middleware is in the pipeline: `UseAuthentication()` → `UseAuthorization()` — never reorder.
- All sensitive endpoints MUST have `[Authorize]` attribute or policy-based authorization.
- Connection strings MUST use User Secrets in Development/Test — never hardcode in `appsettings.json`.
- `appsettings.Production.json` `ConnectionStrings` value MUST be empty string — injected via environment variables at runtime.
- Never log `ICurrentUserService.IpAddress` in plain text without masking.

## ──────────────────────────────────────────────
## 13. DEPENDENCY INJECTION RULES
## ──────────────────────────────────────────────

| Service                         | Lifetime  | Registered in           |
|---------------------------------|-----------|-------------------------|
| MediatR handlers                | Transient | Application (auto)      |
| FluentValidation validators     | Transient | Application (auto scan) |
| IUnitOfWork                     | Scoped    | Infrastructure          |
| IGenericRepository<,>           | Scoped    | Infrastructure          |
| Entity-specific repositories    | Scoped    | Infrastructure          |
| ICurrentUserService             | Scoped    | API / Infrastructure    |
| IHttpContextAccessor            | Singleton | API (AddHttpContextAccessor) |
| Health checks                   | Singleton | Application             |
| DbContext (ClinicHubContext)     | Scoped    | Persistence             |

NEVER register Scoped services as Singleton — it causes captive dependency issues with EF Core.

## ──────────────────────────────────────────────
## 14. ASYNC / THREADING RULES
## ──────────────────────────────────────────────

- ALL I/O-bound operations MUST be async (`await`).
- Always pass and respect `CancellationToken cancellationToken` in:
  - Controller action parameters.
  - Handler `Handle` method.
  - All repository methods.
- NEVER use `.Result` or `.Wait()` — always `await`.
- NEVER use `Task.Run()` for CPU-bound work inside request handlers — offload to background services if needed.
- Use `ConfigureAwait(false)` in library-level code (Application, Domain, Infrastructure) — not needed in ASP.NET Core controllers.

## ──────────────────────────────────────────────
## 15. CODE QUALITY RULES
## ──────────────────────────────────────────────

- Nullable reference types: ENABLED — handle all nullable warnings, do not suppress with `!` without justification.
- Use `record` for DTOs and Commands/Queries (immutability by default).
- Use `sealed` on handler classes — they are not designed for inheritance.
- Use `WhereIfExtension.WhereIf(condition, predicate)` for conditional query filtering.
- Use `ToGuidExtension` for safe `Guid` parsing from strings/objects.
- Prefer expression-bodied members for simple properties and single-line methods.
- Max method length: 40 lines. Extract to private methods if exceeded.
- Max class length: 200 lines. Extract to partial classes or separate classes if exceeded.
- No magic strings — use constants, `LocalizationKeys`, or configuration.
- XML documentation `///` is REQUIRED for all public API surface (controllers, public interfaces, models).

## ──────────────────────────────────────────────
## 16. TESTING RULES (when test project is added)
## ──────────────────────────────────────────────

- Unit test project: `ClinicHub.Tests.Unit`
- Integration test project: `ClinicHub.Tests.Integration`
- Use `xUnit` as the test framework.
- Use `Moq` or `NSubstitute` for mocking.
- Use `FluentAssertions` for readable assertions.
- Test naming: `MethodName_Scenario_ExpectedResult`
  - Example: `Handle_WhenPatientNotFound_ThrowsNotFoundException`
- Every Command/Query handler MUST have unit tests.
- Every validator MUST have unit tests covering valid and invalid inputs.
- Integration tests use `Test` environment (`ASPNETCORE_ENVIRONMENT=Test`).

## ──────────────────────────────────────────────
## 17. GIT / COMMIT RULES
## ──────────────────────────────────────────────

Commit message format: `<type>(<scope>): <description>`

| Type     | Use for                                |
|----------|----------------------------------------|
| feat     | New feature                            |
| fix      | Bug fix                                |
| refactor | Code restructuring (no behavior change)|
| docs     | Documentation only                     |
| test     | Adding or updating tests               |
| chore    | Build, CI, tooling changes             |
| perf     | Performance improvement                |
| migration| Database migration                     |

Examples:
```
feat(patients): add GetPatientByNationalId query
fix(auth): handle null user in CurrentUserService
migration(db): add Patients and Appointments tables
```

Branch naming: `feature/<ticket-id>-short-description` or `fix/<ticket-id>-short-description`

## ──────────────────────────────────────────────
## 18. ENVIRONMENT CONFIGURATION RULES
## ──────────────────────────────────────────────

| Environment | appsettings file              | DB Credentials    | Log Level |
|-------------|-------------------------------|-------------------|-----------|
| Development | appsettings.Development.json  | User Secrets      | Debug     |
| Test        | appsettings.Test.json         | User Secrets      | Information|
| Production  | appsettings.Production.json   | Env Variables     | Warning   |

- `Program.cs` clears default config sources and re-adds in correct order.
- User secrets are loaded for `Development` and `Test` environments only.
- Environment variable prefix: none (plain env vars are merged last).

## ──────────────────────────────────────────────
## 19. SCALAR / OPENAPI DOCUMENTATION RULES
## ──────────────────────────────────────────────

- OpenAPI route: `/openapi/{documentName}.json`
- Scalar UI route: `/scalar/{versionGroupName}` (one per API version)
- Root `/` redirects to the latest version's Scalar UI.
- Every controller action MUST have:
  - `/// <summary>` XML doc comment
  - `[ProducesResponseType]` for every possible HTTP response
- Theme: `ScalarTheme.BluePlanet` — do not change without design approval.

## ──────────────────────────────────────────────
## 20. QUICK-REFERENCE: ADDING A NEW FEATURE
## ──────────────────────────────────────────────

Follow this checklist in order:

```
[ ] 1. Define entity in ClinicHub.Domain/Entities/
[ ] 2. Define repo interface in ClinicHub.Domain/Repositories/Interfaces/
[ ] 3. Add EF configuration in ClinicHub.Persistence/Configuration/ (include HasQueryFilter)
[ ] 4. Implement repo in ClinicHub.Infrastructure/Repositories/Implementations/
[ ] 5. Register repo in ClinicHub.Infrastructure/DependencyInjection.cs
[ ] 6. Create DTO in ClinicHub.Application/Features/{Entity}/DTOs/
[ ] 7. Create Command/Query + Handler in ClinicHub.Application/Features/{Entity}/
[ ] 8. Create Validator for each Command/Query
[ ] 9. Add localization keys to LocalizationKeys.cs + both JSON files
[  ] 10. Create Controller in ClinicHub.API/Controllers/
[ ] 11. Add XML doc + ProducesResponseType to every action
[ ] 12. Create unit tests for handlers and validators
[ ] 13. Add EF migration
[ ] 14. Verify /health endpoint still returns Healthy
[ ] 15. Verify Scalar UI renders the new endpoints correctly
```

## ──────────────────────────────────────────────
## 21. SOCIAL MEDIA API ROUTES
## ──────────────────────────────────────────────

### PostsController
- `GET /api/v2/posts` - Get paginated list of posts
- `GET /api/v2/posts/{id}` - Get post by ID
- `POST /api/v2/posts` - Create a new post
- `PUT /api/v2/posts/{id}` - Update a post
- `DELETE /api/v2/posts/{id}` - Delete a post
- `POST /api/v2/posts/{id}/reactions` - Toggle a reaction on a post

### CommentsController
- `GET /api/v2/posts/{postId}/comments` - Get top-level comments for a post
- `GET /api/v2/comments/{id}/replies` - Get replies for a specific comment
- `POST /api/v2/posts/{postId}/comments` - Add a new comment (or reply if ParentCommentId provided)
- `PUT /api/v2/comments/{id}` - Update a comment
- `DELETE /api/v2/comments/{id}` - Delete a comment
- `POST /api/v2/comments/{id}/reactions` - Toggle a reaction on a comment