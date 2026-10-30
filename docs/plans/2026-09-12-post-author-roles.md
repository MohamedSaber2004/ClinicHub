# Post Author Roles Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Return the author's real role(s) in every `PostDto` instead of a random `roles.FirstOrDefault()`.

**Architecture:** Change `PostDto.UserRole` (`string?`) to `UserRoles` (`IReadOnlyList<string>`) holding ALL Identity roles, populated with one batched `UserRoles + Roles` query (same pattern as `GetAllUsersQueryHandler`) to kill the N+1 `UserManager.GetRolesAsync` loop in the paginated query.

**Tech Stack:** .NET 10, MediatR CQRS, ASP.NET Core Identity (`UserManager<ApplicationUser>`, `IdentityUserRole<Guid>`), EF Core via `IClinicHubContext`, `ClinicHub.slnx`.

---

## File Structure Map

- Modify: `ClinicHub.Application/Features/Posts/DTOs/PostDto.cs` — single responsibility: post response shape. Change `string? UserRole` to `IReadOnlyList<string> UserRoles`.
- Modify: `ClinicHub.Application/Features/Posts/Queries/GetPostById/GetPostByIdQueryHandler.cs:23-43` — single-post path. Replace `roles.FirstOrDefault()` with full sorted role list.
- Modify: `ClinicHub.Application/Features/Posts/Queries/GetPostsPagginated/GetPostsQueryPagginatedHandler.cs:22-75` — paginated path. Replace per-author `GetRolesAsync` loop with one batched `UserRoles`/`Roles` lookup.
- Verify only: `ClinicHub.Application/Features/Posts/Queries/GetPostById/GetPostByIdQuery.cs`, `ClinicHub.Application/Features/Posts/Queries/GetPostsPagginated/GetPostsQueryPagginated.cs` — no changes (query shapes unchanged).
- Verify only: `ClinicHub.Application/Common/Interfaces/IClinicHubContext.cs:24-25` — already exposes `UserRoles` + `Roles`, no change needed.
- Docs to check before coding: `ClinicHub.Application/Features/Users/Queries/GetAllUsers/GetAllUsersQueryHandler.cs:109-115` (canonical batched role-lookup pattern to copy), `ClinicHub.Domain/Enums/UserType.cs` (valid role names: `User, SuperAdmin, Doctor, Staff, ClinicOwner`).

## Decision: single "real" role vs. all roles

The codebase assigns MULTIPLE roles to one user (`RegisterClinicCommandHandler.cs:52-56` adds `ClinicOwner` + `Doctor`; `CreateClinicCommandHandler.cs:83-92` same; `AddUserCommandHandler.cs:92` adds `Doctor` on top of base role). So there is no single "real" role — `FirstOrDefault()` returns whichever row Identity returns first (non-deterministic). This plan implements **all roles** (`IReadOnlyList<string>`), sorted alphabetically for determinism. If the frontend needs one badge, it picks priority client-side (e.g. `Doctor > ClinicOwner > Staff > SuperAdmin > User`).

## Scope Check

Single subsystem (Posts read-model). One plan. Produces working, testable software on its own. No migration needed (Identity tables unchanged, DTO-only change).

---

### Task 1: Change `PostDto` to carry all roles

**Files:**
- Modify: `ClinicHub.Application/Features/Posts/DTOs/PostDto.cs:1-5`
- Test: build check (no test project exists in this repo — see Task 4 for manual verification)

- [ ] **Step 1: Confirm current shape fails the requirement**

Read `ClinicHub.Application/Features/Posts/DTOs/PostDto.cs`. Confirm the last positional parameter is `string? UserRole` — it can only hold ONE role, so a `ClinicOwner + Doctor` user loses one role. No code change in this step.

- [ ] **Step 2: Replace `UserRole` with `UserRoles` list**

Replace the full file content with:

```csharp
namespace ClinicHub.Application.Features.Posts.DTOs
{
    public record PostDto(Guid Id, string Content, Guid AuthorId, string AuthorName, string AuthorProfileImageUrl, DateTime CreatedAt,
                   int ReactionCount, int CommentCount, bool IsFreelanceDoctor, IReadOnlyList<MediaDto> Media, IReadOnlyList<string> UserRoles);
}
```

Why `IReadOnlyList<string>` and not `IList<UserType>`: `UserDto.Roles` uses `IList<UserType>` with enum parsing, but posts only need display strings and must survive unknown future role names without `UserType.None` filtering. Strings keep the contract stable.

- [ ] **Step 3: Build to surface all broken call sites**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: FAIL with `CS1729` / `CS1503` at `GetPostByIdQueryHandler.cs:30` and `GetPostsQueryPagginatedHandler.cs:59` (they still pass `string?`). This failure is the "red" state — it proves you found every constructor call.

- [ ] **Step 4: Commit the DTO change alone (build stays red)**

```bash
git add ClinicHub.Application/Features/Posts/DTOs/PostDto.cs docs/plans/2026-09-12-post-author-roles.md
git commit -m "feat(posts): change PostDto to carry all author roles"
```

Note: commit red on purpose so Task 2/3 each fix one call site and go green independently. Do NOT fix handlers in this task.

---

### Task 2: Return all roles in `GetPostById`

**Files:**
- Modify: `ClinicHub.Application/Features/Posts/Queries/GetPostById/GetPostByIdQueryHandler.cs:21-43`
- Test: `dotnet build` + Scalar `GET /api/v1/posts/{id}`

- [ ] **Step 1: Write the failing expectation (build error is the test)**

No test project exists in this repo (`global.json` pins SDK `10.0.201`, solution has Domain/Application/Persistence/Infrastructure/API only). The failing test for this task is the compiler error from Task 1:

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: FAIL at `GetPostByIdQueryHandler.cs` — `cannot convert from 'string?' to 'IReadOnlyList<string>'`. Copy that error line into your working notes; it is your red state.

- [ ] **Step 2: Replace `FirstOrDefault()` with full sorted list**

Replace lines 28-42 (`var roles = ...` through `roles.FirstOrDefault()`) with:

```csharp
var roles = author != null ? await _userManager.GetRolesAsync(author) : Array.Empty<string>();
var userRoles = roles.OrderBy(r => r).ToList();

return new PostDto(
    post.Id,
    post.Content,
    post.AuthorId,
    author?.FullName ?? "Unknown",
    author?.ProfilePictureUrl ?? string.Empty,
    post.CreatedAt,
    post.Reactions.Count,
    post.Comments.Count,
    doctor != null && doctor.IsFreelance,
    post.Media.Select(m => new MediaDto(m.Id, m.Url, m.Type.ToString())).ToList(),
    userRoles
);
```

Only two lines change: add `var userRoles = roles.OrderBy(r => r).ToList();` and pass `userRoles` instead of `roles.FirstOrDefault()`. Keep `UserManager.GetRolesAsync` here — single-user path has no N+1 problem, so do NOT inject `IClinicHubContext` (YAGNI). `OrderBy` makes output deterministic (Identity returns rows in undefined order).

- [ ] **Step 3: Build to verify this call site is fixed**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: still FAIL, but ONLY at `GetPostsQueryPagginatedHandler.cs:59-70` now. `GetPostByIdQueryHandler` error must be gone. If both errors persist, you edited the wrong lines.

- [ ] **Step 4: Commit**

```bash
git add ClinicHub.Application/Features/Posts/Queries/GetPostById/GetPostByIdQueryHandler.cs
git commit -m "fix(posts): return all author roles in GetPostById"
```

---

### Task 3: Batch role lookup in `GetPostsPagginated`

**Files:**
- Modify: `ClinicHub.Application/Features/Posts/Queries/GetPostsPagginated/GetPostsQueryPagginatedHandler.cs:1-77`
- Test: `dotnet build` + Scalar `GET /api/v1/posts?pageNumber=1&pageSize=10`

- [ ] **Step 1: Confirm the N+1 red state**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: FAIL at `GetPostsQueryPagginatedHandler.cs:55` (`string?` vs `IReadOnlyList<string>`). Also note the perf bug by reading lines 51-56: one `await _userManager.GetRolesAsync(author)` per distinct author = N+1 queries per page. Both are fixed in this task.

- [ ] **Step 2: Add `IClinicHubContext` import and constructor parameter**

Edit the header (lines 1-20). Add one using and one ctor param:

```csharp
using ClinicHub.Application.Common.Interfaces;
```

Change the constructor to:

```csharp
private readonly IClinicHubContext _context;

public GetPostsQueryPagginatedHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IClinicHubContext context)
{
    _unitOfWork = unitOfWork;
    _userManager = userManager;
    _context = context;
}
```

Keep `_userManager` field (still used as fallback) or remove it in Step 3 — either is fine, but do NOT leave an unused private field (compiler warning `CS0414` is not fatal but is sloppy). Recommended: remove `_userManager` entirely since batched query replaces it. If you remove it, delete the `using Microsoft.AspNetCore.Identity;` line too.

- [ ] **Step 3: Replace the per-author loop with one batched lookup**

Replace lines 51-56:

```csharp
var roleLookup = new Dictionary<Guid, string?>();
foreach (var author in page.Items.Select(x => x.user).DistinctBy(u => u.Id))
{
    var roles = await _userManager.GetRolesAsync(author);
    roleLookup[author.Id] = roles.FirstOrDefault();
}
```

with (copy of the proven `GetAllUsersQueryHandler.cs:109-115` pattern):

```csharp
var authorIds = page.Items.Select(x => x.user.Id).Distinct().ToList();

var roleIdToName = await _context.Roles
    .ToDictionaryAsync(r => r.Id, r => r.Name!, cancellationToken);

var userRoleRows = await _context.UserRoles
    .Where(ur => authorIds.Contains(ur.UserId))
    .ToListAsync(cancellationToken);

var roleLookup = userRoleRows
    .GroupBy(ur => ur.UserId)
    .ToDictionary(
        g => g.Key,
        g => (IReadOnlyList<string>)g
            .Select(ur => roleIdToName.GetValueOrDefault(ur.RoleId, string.Empty))
            .Where(name => !string.IsNullOrEmpty(name))
            .OrderBy(name => name)
            .ToList());
```

Then change the projection line 70 from `roleLookup.GetValueOrDefault(x.post.AuthorId)` to:

```csharp
roleLookup.GetValueOrDefault(x.post.AuthorId, Array.Empty<string>())
```

`GetValueOrDefault` with no default returns `null` for missing authors (users with zero roles) which would violate the non-nullable `IReadOnlyList<string>` contract — the two-arg overload avoids that.

- [ ] **Step 4: Build to green**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS with `0 Errors`. If `CS0246 IClinicHubContext not found`, you missed the using in Step 2. If `CS0414 _userManager never used`, remove the field + its using.

- [ ] **Step 5: Commit**

```bash
git add ClinicHub.Application/Features/Posts/Queries/GetPostsPagginated/GetPostsQueryPagginatedHandler.cs
git commit -m "fix(posts): batch all author roles in paginated posts"
```

---

### Task 4: Verify end-to-end + check for stragglers (LAST)

**Files:**
- Verify only: `ClinicHub.API/Controllers/*Post*.cs`, `ClinicHub.API/Routes/ApiRoutes.cs` (no change expected — `PostDto` serializes automatically)
- Test: full build + live Scalar check

- [ ] **Step 1: Prove no other `PostDto(` constructors remain on old shape**

Run in repo root:

```powershell
rg -n "new PostDto\(" --glob "*.cs"
```

Expected: exactly 2 hits — `GetPostByIdQueryHandler.cs` and `GetPostsQueryPagginatedHandler.cs`, both passing a list as last arg. If `CreatePost` or any other file appears, you missed a call site — apply the same `userRoles` list fix there before continuing. (`CreatePostCommandHandler` currently returns `string`, not `PostDto`, so expect no hit there.)

- [ ] **Step 2: Prove no `UserRole` (singular) references remain**

Run:

```powershell
rg -n "UserRole[^s]" ClinicHub.Application/Features/Posts --glob "*.cs"
```

Expected: zero hits (only `UserRoles` plural in `PostDto.cs` + 2 handlers). Any singular hit is a leftover to fix.

- [ ] **Step 3: Full build**

Run: `dotnet build ClinicHub.slnx`
Expected: `Build succeeded. 0 Warning(s) 0 Error(s)`.

- [ ] **Step 4: Manual API verification (TDD substitute — no test project exists)**

Run: `dotnet run --project ClinicHub.API --launch-profile "ClinicHub.API"`
Then open `/scalar/v1`, set `Accept-Language: en`, and call:
1. `GET /api/v1/posts?pageNumber=1&pageSize=10` — every item has `"userRoles": [...]` (array, sorted). A clinic-owner doctor shows `["ClinicOwner","Doctor"]`, not one random string.
2. `GET /api/v1/posts/{id}` with an id from step 1 — same `userRoles` array, same order.
3. Regression: post by a role-less/deleted user shows `"userRoles": []`, never `null`.

- [ ] **Step 5: Commit verification (empty commit if nothing changed)**

```bash
git status --short
```

Expected: clean tree. If stray edits exist, commit them: `git commit --allow-empty -m "test(posts): verify author roles in PostDto"`.

---

## Self-Review

1. **Spec coverage:** "return real role or all roles" → Task 1 (all-roles contract), Task 2 (single-post path), Task 3 (paginated path + N+1 fix), Task 4 (contract + regression check). Covered. The "real single role" alternative is documented as rejected in Decision section with evidence (dual-role assignment in `RegisterClinic`/`CreateClinic`).
2. **Placeholder scan:** no TBD/TODO/"similar to"/"appropriate handling" — every step has exact file:line, full code block, exact command + expected output.
3. **Type consistency:** `IReadOnlyList<string> UserRoles` spelled identically in Tasks 1-3; `roleLookup` is `Dictionary<Guid, IReadOnlyList<string>>` in Task 3 (not the old `Dictionary<Guid, string?>`); `Array.Empty<string>()` default matches the DTO nullability. Consistent.

