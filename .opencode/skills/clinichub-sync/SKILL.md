---
name: clinichub-sync
description: Scan ClinicHub project for new entities, features, or pattern changes and auto-update the clinichub-execution skill to stay in sync. Use periodically or when new implementations/changes are made to the project, to keep the execution skill aligned with the actual codebase. Do NOT use for normal execution or planning tasks.
---

# ClinicHub Sync Skill

## Objective
Detect changes in the ClinicHub codebase — new entities, features, services, refactored patterns — and update the `clinichub-execution` skill's SKILL.md and reference files so the execution skill always reflects the current project state.

## Project Paths

| Path | Description |
|------|-------------|
| `E:\ClinicHub\` | ClinicHub project root |
| `E:\ClinicHub\.agents\skills\clinichub-execution\SKILL.md` | Execution skill to update |
| `E:\ClinicHub\.agents\skills\clinichub-execution\references\` | Reference templates directory |
| `E:\ClinicHub\.agents\skills\clinichub-sync\SKILL.md` | This skill |

## Sync Workflow

### Phase 1: Scan Project Structure

Run multi-threaded scans to collect current state:

| Scan | Target | What to detect |
|------|--------|---------------|
| **Entities** | `ClinicHub.Domain/Entities/*.cs` | New entities, base class changes, new interfaces (`IClinicScopedEntity`), new domain patterns |
| **Enums** | `ClinicHub.Domain/Enums/*.cs` | New enums, `[Flags]` changes, new values |
| **Repo Interfaces** | `ClinicHub.Domain/Repositories/Interfaces/**/*.cs` | New repository interfaces, new method signatures |
| **Features** | `ClinicHub.Application/Features/*/` | New feature folders, new commands/queries |
| **DTOs** | `ClinicHub.Application/Features/*/DTOs/*.cs` | New DTOs, new mapping patterns |
| **Handlers** | `ClinicHub.Application/Features/*/**/*Handler.cs` | New handler patterns, new injected dependencies |
| **Validators** | `ClinicHub.Application/Features/*/**/*Validator.cs` | New validator patterns |
| **AutoMapper** | `ClinicHub.Application/Features/*/*Profile.cs` | New profiles, mapping conventions |
| **EF Configs** | `ClinicHub.Persistence/Configuration/*.cs` | New configs, new Fluent API patterns |
| **Repo Impls** | `ClinicHub.Infrastructure/Repositories/Implementations/**/*.cs` | New implementations |
| **Services** | `ClinicHub.Infrastructure/Services/**/*.cs` | New services, new interface implementations |
| **Controllers** | `ClinicHub.API/Controllers/Version1/*.cs` | New controllers, new attribute patterns, new route patterns |
| **Routes** | `ClinicHub.API/Routes/ApiRoutes.cs` | New route groups, new route patterns |
| **DI** | `ClinicHub.Infrastructure/DependencyInjection.cs`, `ClinicHub.Persistence/DependencyInjection.cs`, `ClinicHub.Application/DependencyInjection.cs` | New registrations, new patterns |
| **Configs** | `ClinicHub.Persistence/Configuration/*.cs` | New table configurations, relationship patterns |
| **Localization** | `ClinicHub.Application/Localization/*.json` | New localization keys |
| **Packages** | `*.csproj` files | New NuGet dependencies |

### Phase 2: Compare with Current Execution Skill

1. **Read** `clinichub-execution/SKILL.md` to get the current documented workflow
2. **Read** `clinichub-execution/references/` for current templates
3. **Diff** each scan result against what the execution skill documents:
   - Are all execution phases still valid?
   - Are there new layer responsibilities or patterns?
   - Are there new dependency injection patterns?
   - Are there new coding conventions or architectural changes?
   - Are there new project references or NuGet packages?

### Phase 3: Identify Changes to Apply

Categorize each detected change:

| Category | Action |
|----------|--------|
| **New entity** | Add entity creation step in Phase 1, add reference template if entity has unique patterns |
| **New enum** | Add enum conventions to Domain step |
| **New feature folder** | Ensure Phase 2 steps cover the feature pattern |
| **New handler pattern** | Update handler template in references/ |
| **New validator pattern** | Update validator template |
| **New DI registration pattern** | Update DI step in Phase 3 |
| **New API attribute/pattern** | Update controller/API steps |
| **Removed/deprecated pattern** | Remove or note as deprecated in execution skill |
| **Refactored pattern** | Update the relevant execution step |
| **Breaking change** | Flag prominently; update affected steps |

### Phase 4: Apply Updates to Execution Skill

1. **Update SKILL.md** — Modify the execution skill's phases, steps, and rules to reflect current project state
2. **Update or add reference templates** — Create/update files in `clinichub-execution/references/`:
   - `command-handler-template.md` — Template for new CQRS command handlers
   - `query-handler-template.md` — Template for new query handlers
   - `validator-template.md` — Template for FluentValidation validators
   - `controller-template.md` — Template for API controllers
   - `entity-template.md` — Template for domain entities
   - `ef-config-template.md` — Template for EF Core configurations
3. **Update scripts** — Update any automation scripts in `clinichub-execution/scripts/`

### Phase 5: Report Summary

Output a clear summary of:
- What was scanned
- What changes were detected
- What was updated in the execution skill
- Any manual review items (ambiguous changes, new patterns that need human judgment)

## Change Detection Rules

- **Entities**: Compare `Domain/Entities/` file listing with the entity patterns documented in execution skill
- **Features**: Compare `Application/Features/` subdirectories with expected feature structure
- **Dependencies**: Parse `*.csproj` files for new packages; note any that change the architecture
- **DI**: Check `DependencyInjection.cs` files for new service lifetimes or registration patterns
- **Conventions**: Look for any new custom attributes, base classes, or architectural patterns

## When Not to Use

- For normal code execution (use `clinichub-execution`)
- For feature planning (use `planning` skill)
- For reviewing code quality

## References
See `references/` for detailed templates and change detection helpers.