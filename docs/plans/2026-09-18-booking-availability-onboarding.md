# Free Onboarding: Booking-Config + Availability Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let a subscription-less clinic owner run their dashboard setup (booking-config add/update/get, per-doctor availability add/update/get/delete) while paid features stay gated.

**Architecture:** Ungate the two booking-config write endpoints, enforce owner-only access with the existing `ClinicAdminId` / new `ClinicScopeResolver` checks, add overlap validation to availability writes, and verify the whole mobile workflow end-to-end. No new endpoints, no migrations, no DI changes.

**Tech Stack:** .NET 10, MediatR CQRS, EF Core + SQL Server + NetTopologySuite, FluentValidation, Scalar API docs.

---

## 0. Conventions (read once, apply to every task)

- **Repo has no test projects.** Verification = `dotnet build` + live endpoint
  checks. Do not create a test project (YAGNI).
- **Build after every edit:**
  `dotnet build ClinicHub.slnx --nologo -v q` — Expected: `Build succeeded. 0 Error(s)`.
- **Run the API for endpoint checks:**
  `dotnet run --project ClinicHub.API` — base URL `https://localhost:44312/api/v1`
  (or the HTTP profile URL printed at startup). Explore endpoints in Scalar at
  `/scalar/v1`. Authenticate as a clinic owner: `POST /api/v1/auth/login` with
  owner credentials, then send `Authorization: Bearer <token>`.
- **Envelope:** success bodies are `ApiResponse<T>` (`{ succeeded, message, data, errors }`).
  Ownership violations are `403 Forbidden` with a localized message. Never `404` for
  cross-clinic access (no id-oracle).
- **Commit after every task** (never batch): `git add <files>`, `git commit -m "<msg>"`.
  Never commit unless the build passes.
- **Read before editing.** Every task lists exact files; open them first.

## 1. File map (all changes)

| # | File | Responsibility |
|---|------|----------------|
| 1 | `ClinicHub.API/Controllers/Version1/BookingConfigurationsController.cs` | Remove 2 `RequirePlanPermission(OnlineBooking)` gates |
| 2 | `ClinicHub.Application/Features/Booking/BookingConfig/Commands/CreateBookingConfig/CreateBookingConfigCommandHandler.cs` | `403` shape + duplicate-config `400` |
| 3 | `ClinicHub.Application/Localization/LocalizationKeys.cs` | Add `BookingConfigAlreadyExists` key |
| 4 | `ClinicHub.Application/Localization/Resources/messages.en.json` | English string for the new key |
| 5 | `ClinicHub.Application/Localization/Resources/messages.ar.json` | Arabic string for the new key |
| 6 | `ClinicHub.Application/Features/Availability/Commands/CreateNewAvailability/CreateNewAvailabilityCommandHandler.cs` | Ownership guard + overlap check |
| 7 | `ClinicHub.Application/Features/Availability/Commands/UpdateExistingAvailability/UpdateExistingAvailabilityCommandHandler.cs` | Ownership guard + null-safety + overlap check |
| 8 | `ClinicHub.Application/Features/Availability/Commands/DeleteAvailability/DeleteAvailabilityCommandHandler.cs` | Ownership guard |
| 9 | `ClinicHub.Application/Features/Availability/Commands/CreateNewAvailability/CreateNewAvailabilityCommandValidator.cs` | Range rules (`start < end`, `slot > 0`) |
| 10 | `ClinicHub.Application/Features/Availability/Commands/UpdateExistingAvailability/UpdateExistingAvailabilityCommandValidator.cs` | Same range rules, null-tolerant |

Read-only references (do NOT modify): `ClinicHub.Application/Common/ClinicScopeResolver.cs`
(resolve helper), `ClinicHub.API/Routes/ApiRoutes.cs` (routes unchanged),
`ClinicHub.API/Controllers/Version1/AvailabilityController.cs` (already ungated).

## 2. Tasks

### Task 1: Ungate booking-config writes

**Files:**
- Modify: `ClinicHub.API/Controllers/Version1/BookingConfigurationsController.cs:45-48` (remove gate on Create)
- Modify: `ClinicHub.API/Controllers/Version1/BookingConfigurationsController.cs:63-66` (remove gate on Update)

- [ ] **Step 1: Remove the two gate attributes**

Delete exactly these two lines (keep `RoleAuthorize` above each):
```csharp
[RequirePlanPermission(SubscriptionPermission.OnlineBooking)]
```
One sits above `[Route(ApiRoutes.BookingConfig.Create)]`, the other above
`[Route(ApiRoutes.BookingConfig.Update)]`. Do NOT touch the GET action.
Then remove the now-unused import at the top of the file:
```csharp
using ClinicHub.Domain.Enums;
```
(`UserType`/`SubscriptionPermission` are no longer referenced in this file;
`RequirePlanPermission` lived in `ClinicHub.API.Filters`, which stays because
`RoleAuthorize` still needs it.)

- [ ] **Step 2: Build**

Run: `dotnet build ClinicHub.slnx --nologo -v q`
Expected: `Build succeeded. 0 Error(s)`. Warnings matching the pre-existing
set are fine; any NEW warning in the edited controller is a failure — fix it.

- [ ] **Step 3: Verify the gate is gone (subscription-less owner)**

Run the API, log in as a clinic owner whose clinic has NO subscription, then:
```powershell
$token = "<owner-jwt>"
$cid = "<owner-clinic-id>"
Invoke-RestMethod -Method Get -Uri "https://localhost:44312/api/v1/clinics/$cid/booking-config" -Headers @{ Authorization = "Bearer $token" }
```
Expected: `200` (config JSON) or `404` (`Booking.ConfigNotFound` — meaning no
config yet, which is the correct first-run state). Then POST a body
`{"consultationFee":300,"maxAdvanceBookingDays":30,"reservationTtlMinutes":10,"cancellationWindowMinutes":120}`.
Expected: `201`. Before this task the POST returned `403`.

- [ ] **Step 4: Commit**

```bash
git add ClinicHub.API/Controllers/Version1/BookingConfigurationsController.cs
git commit -m "feat: make booking-config create/update free for owners (remove OnlineBooking gate)"
```

### Task 2: Booking-create hardening (403 shape + duplicate 400)

**Files:**
- Modify: `ClinicHub.Application/Features/Booking/BookingConfig/Commands/CreateBookingConfig/CreateBookingConfigCommandHandler.cs:24-28`
- Modify: `ClinicHub.Application/Localization/LocalizationKeys.cs:287` (add key after `BookingConfigDeleted`)
- Modify: `ClinicHub.Application/Localization/Resources/messages.en.json` (add string after `"ConfigCreated"`)
- Modify: `ClinicHub.Application/Localization/Resources/messages.ar.json` (add string after `"ConfigCreated"`)

- [ ] **Step 1: Add the `BookingConfigAlreadyExists` key**

In `LocalizationKeys.cs`, inside `BookingMessages`, after line 287, insert:
```csharp
public static readonly KeyString BookingConfigAlreadyExists = new("Booking.ConfigAlreadyExists");
```

- [ ] **Step 2: Add the English string**

In `messages.en.json`, find `"ConfigCreated": "Booking configuration created successfully",`
and insert a new line directly after it:
```json
"ConfigAlreadyExists": "Booking configuration already exists for this clinic. Update it instead.",
```
Keep the trailing comma correct (the inserted line ends with `,` if another key follows).

- [ ] **Step 3: Add the Arabic string**

In `messages.ar.json`, find the `"ConfigCreated"` line and insert directly after it:
```json
"ConfigAlreadyExists": "يوجد إعداد حجز لهذه العيادة بالفعل. قم بتحديثه بدلاً من ذلك.",
```
Same comma rule. Verify the file is still valid JSON (open it; encoding stays UTF-8).

- [ ] **Step 4: Harden the Create handler**

Replace lines 24–27 of `CreateBookingConfigCommandHandler.cs`:
```csharp
var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(request.ClinicId);
if (clinic.ClinicAdminId != _currentUser.UserId)
    throw new UnauthorizedAccessException(LocalizationKeys.ExceptionMessages.Unauthorized.Value);
```
with:
```csharp
var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(request.ClinicId);
if (clinic.ClinicAdminId != _currentUser.UserId)
    throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Unauthorized.Value);

var existing = await _unitOfWork.BookingConfigurationRepository.GetByClinicIdAsync(request.ClinicId);
if (existing != null)
    throw new BadRequestException(LocalizationKeys.BookingMessages.BookingConfigAlreadyExists.Value);
```
Why: `UnauthorizedAccessException` bypasses the app's `ApiResponse` envelope
(raw 500 body); `ForbiddenException` renders the standard 403. The duplicate
check enforces one-config-per-clinic and tells mobile to `PUT`.
`GetByIdAsync` still throws `NotFoundException` for an unknown id (unchanged).

- [ ] **Step 5: Build**

Run: `dotnet build ClinicHub.slnx --nologo -v q`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 6: Verify both behaviors**

As the owner (no subscription): POST the config twice.
Expected: first `201`, second `400` with `ConfigAlreadyExists` message.
As a DIFFERENT owner (or user), POST to the first owner's `{clinicId}`.
Expected: `403` with the standard envelope (`succeeded: false`), not a raw error page.

- [ ] **Step 7: Commit**

```bash
git add ClinicHub.Application/Features/Booking/BookingConfig/Commands/CreateBookingConfig/CreateBookingConfigCommandHandler.cs ClinicHub.Application/Localization/LocalizationKeys.cs ClinicHub.Application/Localization/Resources/messages.en.json ClinicHub.Application/Localization/Resources/messages.ar.json
git commit -m "fix: booking-config create returns standard 403 and rejects duplicates with 400"
```
### Task 3: Availability ownership guards (create/update/delete)

**Files:**
- Modify: `ClinicHub.Application/Features/Availability/Commands/CreateNewAvailability/CreateNewAvailabilityCommandHandler.cs:1-42`
- Modify: `ClinicHub.Application/Features/Availability/Commands/UpdateExistingAvailability/UpdateExistingAvailabilityCommandHandler.cs:1-31`
- Modify: `ClinicHub.Application/Features/Availability/Commands/DeleteAvailability/DeleteAvailabilityCommandHandler.cs:22-29`

Background (do not change): all three handlers load entities with
`GetByIdAsync` (`FindAsync`), which does NOT apply the tenant query filter —
so an explicit scope check is required. Resolve the caller's clinic with the
shared `ClinicScopeResolver` (token claim, else DB fallback), exactly like the
dashboard handlers do.

- [ ] **Step 1: Guard Create**

Edit `CreateNewAvailabilityCommandHandler.cs`:
1. Add constructor parameter `ICurrentUserService currentUser` (store in
   `_currentUser`) and usings `ClinicHub.Application.Common`,
   `ClinicHub.Application.Common.Interfaces`, `ClinicHub.Application.Localization`.
2. After line 26 (`doctor.ClinicId == null` check), insert:
```csharp
var scopeClinicId = await ClinicScopeResolver.ResolveClinicIdAsync(
    _unitOfWork, _currentUser.UserId, _currentUser.CurrentClinicId, cancellationToken);
if (!scopeClinicId.HasValue || doctor.ClinicId.Value != scopeClinicId.Value)
    throw new ForbiddenException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);
```
Why `ClinicNotFound` (not Unauthorized): it matches the message mobile already
handles, and it reveals nothing about other clinics' doctors.

- [ ] **Step 2: Build**

Run: `dotnet build ClinicHub.slnx --nologo -v q`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 3: Guard Update (+ null-safety)**

Edit `UpdateExistingAvailabilityCommandHandler.cs`:
1. Add `ICurrentUserService` + the same three usings as Step 1.
2. After loading `availability` (line 22), insert the same resolver block, but
   compare `availability.ClinicId`:
```csharp
var scopeClinicId = await ClinicScopeResolver.ResolveClinicIdAsync(
    _unitOfWork, _currentUser.UserId, _currentUser.CurrentClinicId, cancellationToken);
if (!scopeClinicId.HasValue || availability.ClinicId != scopeClinicId.Value)
    throw new ForbiddenException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);
```
3. Replace the `availability.Update(...)` line with null-tolerant fallback
   (today `request.DayOfWeek!.Value` throws a raw NRE when mobile omits a field):
```csharp
availability.Update(
    request.DayOfWeek ?? availability.DayOfWeek,
    request.StartTime ?? availability.StartTime,
    request.EndTime ?? availability.EndTime,
    request.SlotDurationMinutes ?? availability.SlotDurationMinutes);
```

- [ ] **Step 4: Build**

Run: `dotnet build ClinicHub.slnx --nologo -v q`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 5: Guard Delete**

In `DeleteAvailabilityCommandHandler.cs`, after line 25 (`GetByIdAsync`), insert:
```csharp
var scopeClinicId = await ClinicScopeResolver.ResolveClinicIdAsync(
    _unitOfWork, _currentUserService.UserId, _currentUserService.CurrentClinicId, cancellationToken);
if (!scopeClinicId.HasValue || availability.ClinicId != scopeClinicId.Value)
    throw new ForbiddenException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);
```
Add usings `ClinicHub.Application.Common` and `ClinicHub.Application.Common.Exceptions`
(`Interfaces` and `Localization` are already imported).

- [ ] **Step 6: Build + verify cross-clinic writes are 403**

Run: `dotnet build ClinicHub.slnx --nologo -v q`
Expected: `Build succeeded. 0 Error(s)`.
Then, as owner A, `PUT /availability/{id-of-owner-B-window}` and
`DELETE /availability/{id-of-owner-B-window}`.
Expected: both `403` with the standard envelope. As owner B on their own
window: `200`.

- [ ] **Step 7: Commit**

```bash
git add ClinicHub.Application/Features/Availability/Commands/CreateNewAvailability/CreateNewAvailabilityCommandHandler.cs ClinicHub.Application/Features/Availability/Commands/UpdateExistingAvailability/UpdateExistingAvailabilityCommandHandler.cs ClinicHub.Application/Features/Availability/Commands/DeleteAvailability/DeleteAvailabilityCommandHandler.cs
git commit -m "fix: scope availability writes to the caller's own clinic (403 otherwise)"
```
### Task 4: Availability overlap + range validation

**Files:**
- Modify: `ClinicHub.Application/Localization/LocalizationKeys.cs:187` (add key after `NotOwnedByDoctor`)
- Modify: `ClinicHub.Application/Localization/Resources/messages.en.json` (add string after `"NotOwnedByDoctor"`)
- Modify: `ClinicHub.Application/Localization/Resources/messages.ar.json` (add string after `"NotOwnedByDoctor"`)
- Modify: `ClinicHub.Application/Features/Availability/Commands/CreateNewAvailability/CreateNewAvailabilityCommandValidator.cs:34-53` (add overlap rule)
- Modify: `ClinicHub.Application/Features/Availability/Commands/UpdateExistingAvailability/UpdateExistingAvailabilityCommandValidator.cs:30-56` (add overlap rule)

Background: range rules (`start < end`, `slot 0..480`, within-clinic-schedule)
already exist in both validators — do not touch them. What is missing is the
no-overlap rule: two windows for the same doctor on the same day must not
intersect. Overlap test: `newStart < existingEnd && existingStart < newEnd`.

- [ ] **Step 1: Add the `Availability.Overlap` key + strings**

In `LocalizationKeys.cs`, inside `AvailabilityMessages`, after line 187 insert:
```csharp
public static readonly KeyString Overlap = new("Availability.Overlap");
```
In `messages.en.json`, after the `"NotOwnedByDoctor"` line insert:
```json
"Overlap": "This time window overlaps an existing availability for this doctor on the same day.",
```
In `messages.ar.json`, after the `"NotOwnedByDoctor"` line insert:
```json
"Overlap": "هذه الفترة تتداخل مع فترة متاحة موجودة لهذا الطبيب في نفس اليوم.",
```
Keep commas/UTF-8 intact; open each JSON after editing to confirm validity.

- [ ] **Step 2: Overlap rule on Create**

In `CreateNewAvailabilityCommandValidator.cs`, after the `WithinClinicSchedule`
rule (lines 34–37), insert:
```csharp
RuleFor(x => x)
    .MustAsync(NoOverlap)
    .WithName("Availability")
    .WithMessage(localizer[LocalizationKeys.AvailabilityMessages.Overlap]);
```
and add the method after `WithinClinicSchedule` (after line 53):
```csharp
private async Task<bool> NoOverlap(CreateNewAvailabilityCommand command, CancellationToken cancellationToken)
{
    return !await _ctx.DoctorAvailabilityRepository.ExistsAsync(a =>
        !a.IsDeleted &&
        a.DoctorId == command.DoctorId &&
        a.DayOfWeek == command.DayOfWeek &&
        a.StartTime < command.EndTime &&
        a.EndTime > command.StartTime,
        cancellationToken);
}
```
`ExistsAsync` takes `(predicate, cancellationToken)` — same shape as the
existing `DoctorExists` method in this file.

- [ ] **Step 3: Build**

Run: `dotnet build ClinicHub.slnx --nologo -v q`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 4: Overlap rule on Update (exclude self)**

In `UpdateExistingAvailabilityCommandValidator.cs`, after the
`WithinClinicSchedule` rule (lines 30–33), insert:
```csharp
RuleFor(x => x)
    .MustAsync(NoOverlap)
    .WithName("Availability")
    .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.AvailabilityMessages.Overlap.Value]));
```
Match this file's existing style (`JsonLocalizationProvider.GetLocalizedString(... .Value)`).
Add after `WithinClinicSchedule` (after line 56):
```csharp
private async Task<bool> NoOverlap(UpdateExistingAvailabilityCommand command, CancellationToken cancellationToken)
{
    var availability = await _ctx.DoctorAvailabilityRepository.GetByIdAsync(command.Id);
    if (availability is null)
        return true; // Reported by the AvailabilityExists rule.

    var day = command.DayOfWeek ?? availability.DayOfWeek;
    var start = command.StartTime ?? availability.StartTime;
    var end = command.EndTime ?? availability.EndTime;

    return !await _ctx.DoctorAvailabilityRepository.ExistsAsync(a =>
        !a.IsDeleted &&
        a.Id != command.Id &&
        a.DoctorId == availability.DoctorId &&
        a.DayOfWeek == day &&
        a.StartTime < end &&
        a.EndTime > start,
        cancellationToken);
}
```
Note `a.Id != command.Id` — without it, every update would collide with itself.

- [ ] **Step 5: Build + verify overlap end-to-end**

Run: `dotnet build ClinicHub.slnx --nologo -v q`
Expected: `Build succeeded. 0 Error(s)`.
Then as the owner: `POST /availability` Saturday `09:00–12:00` → `200`;
POST Saturday `11:00–13:00` for the same doctor → `400` with the Overlap
message. POST Saturday `12:00–14:00` (touching edge, not overlapping) → `200`.

- [ ] **Step 6: Commit**

```bash
git add ClinicHub.Application/Localization/LocalizationKeys.cs ClinicHub.Application/Localization/Resources/messages.en.json ClinicHub.Application/Localization/Resources/messages.ar.json ClinicHub.Application/Features/Availability/Commands/CreateNewAvailability/CreateNewAvailabilityCommandValidator.cs ClinicHub.Application/Features/Availability/Commands/UpdateExistingAvailability/UpdateExistingAvailabilityCommandValidator.cs
git commit -m "feat: reject overlapping availability windows per doctor/day"
```
### Task 5: End-to-end dashboard workflow verification

**Files:** none (verification only — if anything fails, fix it under its own task, do not bundle).

Prerequisites: Tasks 1–4 committed, API running (`dotnet run --project ClinicHub.API`),
an approved owner account with NO subscription, its clinic id, and one doctor in
that clinic. Base: `https://localhost:44312/api/v1`.

- [ ] **Step 1: Log in and capture the token**

```powershell
$login = Invoke-RestMethod -Method Post -Uri "https://localhost:44312/api/v1/auth/login" -ContentType "application/json" -Body '{"email":"<owner-email>","password":"<owner-password>"}'
$token = $login.data.accessToken
$cid = $login.data.clinicId
$H = @{ Authorization = "Bearer $token" }
```
Expected: `200`, non-empty token and `clinicId`.

- [ ] **Step 2: Booking-config first-run cycle**

```powershell
try { Invoke-RestMethod -Method Get -Uri "https://localhost:44312/api/v1/clinics/$cid/booking-config" -Headers $H } catch { $_.Exception.Response.StatusCode.value__ }
```
Expected: `404` (no config yet).
```powershell
$body = '{"consultationFee":300,"maxAdvanceBookingDays":30,"reservationTtlMinutes":10,"cancellationWindowMinutes":120}'
Invoke-RestMethod -Method Post -Uri "https://localhost:44312/api/v1/clinics/$cid/booking-config" -Headers $H -ContentType "application/json" -Body $body
Invoke-RestMethod -Method Get -Uri "https://localhost:44312/api/v1/clinics/$cid/booking-config" -Headers $H
Invoke-RestMethod -Method Put -Uri "https://localhost:44312/api/v1/clinics/$cid/booking-config" -Headers $H -ContentType "application/json" -Body $body.Replace("300","350")
```
Expected: `201` with `currency: EGP`, then `200` echoing the row, then `200`
with `consultationFee: 350`. POST the same body again → `400` ConfigAlreadyExists.

- [ ] **Step 3: Availability week cycle**

```powershell
$did = "<doctor-id-in-own-clinic>"
Invoke-RestMethod -Method Get -Uri "https://localhost:44312/api/v1/availability?doctorId=$did&clinicId=$cid" -Headers $H
$slot = '{"doctorId":"'$did'","dayOfWeek":6,"startTime":"09:00:00","endTime":"14:00:00","slotDurationMinutes":30}'
$created = Invoke-RestMethod -Method Post -Uri "https://localhost:44312/api/v1/availability" -Headers $H -ContentType "application/json" -Body $slot
$sid = $created.data.id
Invoke-RestMethod -Method Put -Uri "https://localhost:44312/api/v1/availability/$sid" -Headers $H -ContentType "application/json" -Body '{"dayOfWeek":6,"startTime":"10:00:00","endTime":"15:00:00","slotDurationMinutes":30}'
Invoke-RestMethod -Method Delete -Uri "https://localhost:44312/api/v1/availability/$sid" -Headers $H
```
Expected: `200` at every step; the GET shows the window between POST and DELETE.

- [ ] **Step 4: Paid gates still closed**

```powershell
try { Invoke-RestMethod -Method Get -Uri "https://localhost:44312/api/v1/admin/clinics/advanced-report?clinicId=$cid" -Headers $H } catch { $_.Exception.Response.StatusCode.value__ }
```
Expected: `403` (AdvancedReports still requires a subscription). Any `200` here
is a regression — stop and fix before finishing.

- [ ] **Step 5: Confirm clean tree and finish**

Run: `git status --short`
Expected: only the two plan/spec docs from this design cycle are untracked, no
source diffs remain. Commit them:
```bash
git add docs/specs/2026-09-18-booking-availability-onboarding-design.md docs/plans/2026-09-18-booking-availability-onboarding.md
git commit -m "docs: onboarding booking-config + availability design and plan"
```
