---
name: clinichub-execution
description: Execute code in ClinicHub following Clean Architecture, CQRS, and project conventions. Use when the user says "نفذ", "اكتب الكود", "طبّق", "ابدأ implementation", "execute", "implement", "write code", or requests actual code generation based on a plan or directly. Do NOT use for planning-only discussions or code review.
---

# ClinicHub Execution Skill

## Objective
Execute code systematically, step-by-step, following ClinicHub's Clean Architecture layers, CQRS patterns, and established conventions. Verify each step before proceeding.

## Core Project Structure
```
ClinicHub.Domain/          # Entities, enums, repository interfaces
ClinicHub.Application/     # MediatR commands/queries, DTOs, validators, AutoMapper profiles
  Features/<Feature>/
    Commands/<Action>/
    Queries/<Action>/
    DTOs/
    *Profile.cs
  Common/                  # Behaviours, exceptions, interfaces, models
ClinicHub.Persistence/     # DbContext, EF configurations, migrations, seeders
  Configuration/
ClinicHub.Infrastructure/  # Repository implementations, external services
  Repositories/Implementations/
ClinicHub.API/             # Controllers, routes, middleware
  Controllers/Version1/
  Routes/ApiRoutes.cs
```

## Execution Phases

### Phase 0: Pre-Execution Check
- If a plan exists (from planning skill), confirm it and follow it step by step
- If no plan exists, summarize in 1-2 sentences what you will execute before starting
- Identify which layers need changes (Domain, Application, Persistence, Infrastructure, API)

### Phase 1: Domain & Persistence Layer

**Step 1: Domain Entity** (`ClinicHub.Domain/Entities/`)
- Inherit from `BaseEntity<Guid>` (or `BaseEntity` if no key needed)
- Use private setters, domain methods for state changes
- Collections: `private readonly List<T> _items` field, expose as `IReadOnlyCollection<T>`
- Add `IClinicScopedEntity` if clinic-scoped multi-tenancy needed

**Step 2: Enum (optional)** (`ClinicHub.Domain/Enums/`)
- Use `[Flags]` attribute if bitmask combination needed
- Follow existing naming conventions

**Step 3: Repository Interface** (`ClinicHub.Domain/Repositories/Interfaces/`)
- Inherit `IGenericRepository<TEntity, Guid>` (or custom key type)
- Add custom query methods only if standard generic methods are insufficient

**Step 4: EF Configuration** (`ClinicHub.Persistence/Configuration/`)
- Implement `IEntityTypeConfiguration<TEntity>`
- Required: `HasQueryFilter(x => !x.IsDeleted)` for soft delete
- Required: `IsRowVersion()` on `Version` property for concurrency
- Use `PropertyAccessMode.Field` for `IReadOnlyCollection` backing fields
- Use `OnDelete(DeleteBehavior.Restrict)` to prevent cascade deletes

**Step 5: Repository Implementation** (`ClinicHub.Infrastructure/Repositories/Implementations/`)
- Inherit `GenericRepository<TEntity, Guid>`, implement the interface
- Only needed if custom methods beyond generic CRUD are required

**Step 6: Unit of Work Registration**
- Add property to `IUnitOfWork` (in `Domain/Repositories/Interfaces/`)
- Implement in `UnitOfWork` (in `Infrastructure/Repositories/Implementations/`) using lazy initialization pattern

**Step 7: DbContext Registration** (if new DbSet needed)
- Add `public DbSet<TEntity> TEntities { get; set; }` to `ClinicHubContext`
- Apply configuration in `OnModelCreating` or rely on `ApplyConfigurationsFromAssembly`

### Phase 2: Application Layer

**Step 8: DTO** (`ClinicHub.Application/Features/<Feature>/DTOs/`)
- Use `class` with `{ get; set; }` properties (most common pattern). `record` is also used for simple read-only DTOs.
- Follow existing naming conventions (`PostDto`, `ClinicDto`, `CreateDoctorAvailabilityDto`)

**Step 9: AutoMapper Profile** (`ClinicHub.Application/Features/<Feature>/`)
- Create `<Feature>Profile.cs` extending `Profile`
- Define `CreateMap<TEntity, TDto>()` and reverse maps as needed

**Step 10: Command/Query + Handler** (`ClinicHub.Application/Features/<Feature>/Commands/<Action>/` or `Queries/<Action>/`)
- Commands: `class <Action>Command : IRequest<TResponse>` with `{ get; set; }` properties (preferred). `record` is also used for simple positional commands.
- Queries: `class <Action>Query : IRequest<TResponse>` — return DTO or `PagginatedResult<T>`
- Handlers: implement `IRequestHandler<TRequest, TResponse>`
- Common injected dependencies: `IUnitOfWork`, `ICurrentUserService`, `IMapper`, `IStringLocalizer<Messages>`
- Get current user: `_currentUserService.UserId`, `_currentUserService.CurrentClinicId`
- For localization: `localizer[LocalizationKeys.SomeKey]`
- A single handler can orchestrate multiple repository operations in one transaction (e.g., create an entity then create related child entities)

**Step 11: Validator** (embedded in Command/Query file or separate)
- Extend `AbstractValidator<T>` 
- Inject `IStringLocalizer<Messages>` for error messages
- Use `RuleFor(x => x.Property).NotEmpty().WithMessage(localizer[LocalizationKeys.SomeKey])` (no `JsonLocalizationProvider` wrapper needed in newer validators)
- For list properties: `RuleForEach(x => x.List).ChildRules(item => { item.RuleFor(...)... })` to validate each item
- Validators auto-run via `ValidationBehaviour` pipeline — no manual invocation needed

### Phase 3: API Layer

**Step 12: Routes** (`ClinicHub.API/Routes/ApiRoutes.cs`)
- Add route constants as nested static classes in `ApiRoutes`
- Pattern: `public const string Create = "api/v{version:apiVersion}/resource"`

**Step 13: Controller** (`ClinicHub.API/Controllers/Version1/`)
- Inherit `BaseApiController`
- Use `[ApiVersion("1.0")]` and `[RoleAuthorize(UserType.Admin | UserType.Doctor)]` or `[AllowAnonymous]`
- Actions call `await Mediator.Send(command/query)` — use the inherited `Mediator` property from `BaseApiController`
- Return using base helpers: `Ok(result)`, `Created(result)`, `Deleted(result)`, `Accepted(result)`
- `Ok()` wraps in `ApiResponse<T>.Ok()`, `Created()` returns 201, `Deleted()` returns 200 with localized message

**Step 14: DI Registration** (if new service added)
- Commands/queries/handlers/validators are auto-registered via MediatR in `ClinicHub.Application/DependencyInjection.cs` — no manual registration needed
- For new repositories: register in `ClinicHub.Infrastructure/DependencyInjection.cs` via `services.AddScoped<IMyRepo, MyRepo>()`
- For new services: register in `ClinicHub.Infrastructure/DependencyInjection.cs` via `services.AddScoped<IMyService, MyService>()`

### Phase 4: Verification
- Run `dotnet build` to check compilation
- Verify all necessary references/imports are present
- Confirm no breaking changes to existing contracts
- Summarize what was executed and what remains (if any)

## Execution Rules

1. **One step at a time** — Do not write all files at once for large tasks. Verify after each logical block before moving on.

2. **Follow project conventions**
   - Namespaces match folder path: `ClinicHub.API.Controllers.Version1`
   - Private fields: `_camelCase`
   - Async methods suffixed with `Async`
   - All DB operations via repositories/UnitOfWork, never direct DbContext in Application layer
   - Use `IStringLocalizer<Messages>` for all user-facing strings
   - Return `ApiResponse<T>` from controllers, `Result<T>` from handlers

3. **No unnecessary dependencies** — Do not introduce new NuGet packages without clear justification.

4. **On failure**
   - Do not ignore or hide the error
   - Explain the cause and suggest the next step
   - Fix the issue before continuing

## References
See `references/` for detailed templates and examples.