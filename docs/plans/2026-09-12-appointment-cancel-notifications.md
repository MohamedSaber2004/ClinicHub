# Appointment Cancellation Notifications Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Notify the clinic side on manual patient cancellation, and automatically void no-show appointments (30-min grace via Hangfire) without ever voiding checked-in patients.

**Architecture:** Presence is tracked with a new nullable `CheckedInAt` on `Appointment` (set by `CheckIn()`); the existing per-entity `NoShowJob` and a new hourly `NoShowSweepJob` safety net both skip checked-in patients and notify patient + doctor + owner. Manual cancel keeps its patient notice and adds a clinic-side notice reusing the existing `AppointmentCancellation` template.

**Tech Stack:** .NET 10, MediatR CQRS, EF Core + Npgsql (PostgreSQL, connection `CareClinicHubDb`), Hangfire (`IBackgroundJobScheduler`, `BackgroundJob.Schedule`, `RecurringJob`), FCM via `IFcmService.SendToUserAsync` (persists `Notification` rows via `NotificationBuilderService`).

---

## Scope Check

One plan (not two): manual-cancel notification and auto-no-show share the same core (cancellation semantics + the FCM/row notification pattern). Each task below still produces working, testable software on its own. No new API endpoints, no controller/route changes, no frontend changes (clients already switch on the `type` string in the notification payload).

## Key Decisions (read before coding)

1. **Auto outcome is `NoShow`, not `Cancelled`.** The spec says "cancel automatically", but this codebase tracks `NoShow` (5) as the distinct terminal auto-state: `CancelAppointment` refuses to touch it, the doctor dashboard has a dedicated no-show action (code 5), and reports distinguish it. Voiding = the slot is freed and the booking is terminal. If product truly wants status 2, replace `MarkNoShow()` with `Cancel(reason)` in exactly the two call sites in Tasks 4-5.
2. **Grace = `AppointmentDate + EndTime + 30 minutes`, server local time (`DateTime.Now`).** Matches the already-scheduled per-entity jobs (`VerifyBookingPayment:65`, `ConfirmPaymentWebhook:108`, `AppointmentAcceptanceService:102`) and the existing `NoShowJob` guard. Timezone-free convention per `Appointment.cs:57-58`.
3. **Manual-cancel clinic notice reuses `AppointmentCancellation`.** No new enum member: it IS a cancellation, and the template takes a free `reason` string — prefix it with the cancelling patient's name. Only the auto path gets a new type (`AppointmentNoShow = 21`) because clients badge/filter no-shows separately from cancels.
4. **No `messages.{en,ar}.json` changes.** Push templates are hardcoded Arabic in `NotificationBuilderService`; the API `localizer` path is untouched. Do not add JSON keys.
5. **Checked-in patients must never auto-void.** Today `CheckIn()` sets `Confirmed`, which `NoShowJob` still voids — the core presence bug this plan fixes.

## File Structure Map

- Modify: `ClinicHub.Domain/Entities/Appointment.cs` — add `CheckedInAt DateTime?`; `CheckIn()` stamps it; `Update()` clears it when the slot changes.
- Modify: `ClinicHub.Persistence/Configuration/AppointmentConfiguration.cs` — map `CheckedInAt` (mirrors `ExpiresAt` line 29).
- Migrate: `ClinicHub.Persistence/Migrations/` (PostgreSQL set; `Migrations/SqlServer/` is inactive legacy — do NOT add there).
- Modify: `ClinicHub.Application/Features/Appointments/Commands/CancelAppointment/CancelAppointmentCommandHandler.cs:155-178` — add doctor+owner notice after the patient notice.
- Modify: `ClinicHub.Domain/Enums/NotificationType.cs` — append `AppointmentNoShow = 21`.
- Modify: `ClinicHub.Infrastructure/Services/NotificationBuilderService.cs:88-94` — add the `AppointmentNoShow` case after `AppointmentCancellation`.
- Modify: `ClinicHub.Infrastructure/Services/BackgroundJobs/NoShowJob.cs` — skip checked-in, notify all three parties.
- Create: `ClinicHub.Infrastructure/Services/BackgroundJobs/NoShowSweepJob.cs` — hourly safety-net sweep (same guards; self-heals reschedules and lost one-off jobs).
- Modify: `ClinicHub.Infrastructure/DependencyInjection.cs:107` — register the sweeper next to `NoShowJob`.
- Modify: `ClinicHub.API/Program.cs:329-332` — register `"noshow-sweep"` with `Cron.Hourly` (job classes are already imported; no new using needed — verify via build).
- Docs to check before coding: `AppointmentAcceptanceService.cs:135-178` (canonical doctor+owner recipient pattern to copy), `CancellationWindowJob.cs:22-47` (canonical send-from-job + single-commit pattern), `AbandonedPaymentJob.cs:22-47` (canonical sweep structure).

---

### Task 1: Presence tracking on the domain entity

**Files:**
- Modify: `ClinicHub.Domain/Entities/Appointment.cs:34,105-111`
- Modify: `ClinicHub.Persistence/Configuration/AppointmentConfiguration.cs:29`

- [ ] **Step 1: Add `CheckedInAt` property after `ExpiresAt`**

Edit `Appointment.cs` — insert one line after line 34 (`public DateTime? ExpiresAt { get; private set; }`):

```csharp
public DateTime? CheckedInAt { get; private set; }
```

- [ ] **Step 2: Stamp arrival in `CheckIn()`**

Replace lines 105-109:

```csharp
public void CheckIn()
{
    Status = AppointmentStatus.Confirmed;
    CheckedInAt = DateTime.Now;
    ExpiresAt = null;
}
```

`DateTime.Now` (server local) matches every other timestamp in this flow (`DeleteUser:35`, `NoShowJob:31`). Walk-ins call `CheckIn()` at creation (`RegisterWalkInPatient:82`) so they are correctly stamped present.

- [ ] **Step 3: Map the column**

Edit `AppointmentConfiguration.cs` — after line 29 (`builder.Property(x => x.ExpiresAt);`) add:

```csharp
builder.Property(x => x.CheckedInAt);
```

Nullable `DateTime?` needs no further config (mirrors `ExpiresAt`).

- [ ] **Step 4: Build to verify (migration comes in Task 2, so DB is untouched)**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors. (New property is unused so far — no behavior change.)

- [ ] **Step 5: Commit**

```bash
git add ClinicHub.Domain/Entities/Appointment.cs ClinicHub.Persistence/Configuration/AppointmentConfiguration.cs
git commit -m "feat(appointments): track patient check-in time for no-show guard"
```

---

### Task 2: EF migration for CheckedInAt

**Files:**
- Create (via tooling): `ClinicHub.Persistence/Migrations/<timestamp>_AddCheckedInAtToAppointment.cs`
- Test: `dotnet ef migrations list`, `dotnet build`

- [ ] **Step 1: Generate the migration (PostgreSQL set only)**

Run from repo root:

```bash
dotnet ef migrations add AddCheckedInAtToAppointment --project ClinicHub.Persistence --startup-project ClinicHub.API
```

Expected: two new files under `ClinicHub.Persistence/Migrations/` (NOT under `Migrations/SqlServer/`, which is inactive legacy — active provider is Npgsql per `ClinicHub.Persistence/DependencyInjection.cs:16`). Inspect the `Up()` — it must contain exactly one operation:

```csharp
migrationBuilder.AddColumn<DateTime>(
    name: "CheckedInAt",
    table: "Appointments",
    nullable: true);
```

If `Up()` contains anything else, stop — the tooling picked up unrelated model drift. Do not hand-edit; report it.

- [ ] **Step 2: Verify migration list + build**

Run: `dotnet ef migrations list --project ClinicHub.Persistence --startup-project ClinicHub.API`
Expected: `AddCheckedInAtToAppointment` appears last with `(Pending)`.

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors.

- [ ] **Step 3: Commit (migration applies itself at deploy; do NOT run `database update` against shared DBs)**

```bash
git add ClinicHub.Persistence/Migrations/*AddCheckedInAtToAppointment* ClinicHub.Persistence/Migrations/ClinicHubContextModelSnapshot.cs
git commit -m "feat(appointments): migration for CheckedInAt column"
```

### Task 3: Clinic-side notice on manual cancel

**Files:**
- Modify: `ClinicHub.Application/Features/Appointments/Commands/CancelAppointment/CancelAppointmentCommandHandler.cs:46-50,155-178`

Today only the patient is notified. The doctor and clinic admin learn about the freed slot from nothing. Copy the recipient pattern from `AppointmentAcceptanceService.cs:159-170` (HashSet dedupe of `Doctor.UserId` + `ClinicAdminId`).

- [ ] **Step 1: Include `Doctor` in the load (needed for `Doctor.UserId`)**

Replace lines 46-50:

```csharp
var appointment = await _unitOfWork.AppointmentRepository
    .GetFirstWithIncluding(
        a => a.Id == request.AppointmentId,
        a => a.Clinic!,
        a => a.Doctor)
    .FirstOrDefaultAsync(cancellationToken);
```

- [ ] **Step 2: Notify doctor + owner right after the patient notice**

Insert this block between the patient-notify `try/catch` (ends line 166) and the second `SaveChangesAsync` `try` (starts line 168). It must sit BEFORE that save so the clinic rows persist in the same commit:

```csharp
try
{
    var recipients = new HashSet<Guid>();
    if (appointment.Doctor != null)
        recipients.Add(appointment.Doctor.UserId);
    if (appointment.Clinic?.ClinicAdminId.HasValue == true)
        recipients.Add(appointment.Clinic.ClinicAdminId.Value);
    recipients.Remove(appointment.BookedByUserId);

    foreach (var userId in recipients)
    {
        await _fcmService.SendToUserAsync(userId, NotificationType.AppointmentCancellation, new()
        {
            ["clinicName"] = appointment.Clinic?.Name ?? "",
            ["reason"] = $"Patient {appointment.PatientFullName} cancelled: {request.CancellationReason}",
            ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
            ["appointmentId"] = appointment.Id.ToString()
        });
    }
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to send clinic-side cancellation notification for appointment {AppointmentId}.", appointment.Id);
}
```

Notes: `PatientFullName` lives on the entity (no user lookup needed). `recipients.Remove(BookedByUserId)` prevents double-notifying the patient with clinic wording. Same `AppointmentCancellation` type per Decision 3 — no enum/builder change. Notification failures never fail the cancel (mirrors the patient block).

- [ ] **Step 3: Build**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors.

- [ ] **Step 4: Manual verify via Scalar (needs running API + test DB)**

Run: `dotnet run --project ClinicHub.API --launch-profile "ClinicHub.API"`, open `/scalar/v1`, cancel an appointment as the patient, then as the clinic admin call the notifications list endpoint (or inspect the `Notifications` table). Expected: TWO new rows with `Type = AppointmentCancellation` — one for the patient (`reason` = raw text), one for the admin (`reason` starts with `Patient {name} cancelled:`).

- [ ] **Step 5: Commit**

```bash
git add ClinicHub.Application/Features/Appointments/Commands/CancelAppointment/CancelAppointmentCommandHandler.cs
git commit -m "feat(appointments): notify clinic side on manual patient cancellation"
```

### Task 4: Auto no-show notifies everyone + respects check-in

**Files:**
- Modify: `ClinicHub.Domain/Enums/NotificationType.cs:20`
- Modify: `ClinicHub.Infrastructure/Services/NotificationBuilderService.cs:88-94`
- Modify: `ClinicHub.Infrastructure/Services/BackgroundJobs/NoShowJob.cs:22-38`

- [ ] **Step 1: Add the `AppointmentNoShow` notification type**

Edit `NotificationType.cs` — after line 20 (`SubscriptionActivated = 20`) add:

```csharp
AppointmentNoShow = 21,
```

`21` is the next free value (`16` is skipped historically; do not reuse it). Int-backed enum: no migration needed.

- [ ] **Step 2: Add the push template right after `AppointmentCancellation`**

Edit `NotificationBuilderService.cs` — insert after the `AppointmentCancellation` case (ends line 94):

```csharp
case NotificationType.AppointmentNoShow:
    title = "تسجيل عدم حضور";
    body = $"تغيب {GetParam(parameters, "patientName")} عن موعده في {clinicName} بتاريخ {date} الساعة {time}";
    data["patientName"] = GetParam(parameters, "patientName");
    data["clinicName"] = clinicName;
    data["date"] = date;
    data["time"] = time;
    data["appointmentId"] = appointmentId;
    if (!string.IsNullOrEmpty(appointmentId))
        link = _deepLinkService.GenerateLink(string.Format(DeepLinkRoutes.AppointmentDetails, appointmentId));
    else
        link = _deepLinkService.GenerateLink(DeepLinkRoutes.Appointments);
    break;
```

Third-person wording with `patientName` reads correctly for patient, doctor, and owner alike. `DeepLinkRoutes.AppointmentDetails` already exists (used by neighboring cases).

- [ ] **Step 3: Build the type + template (job wiring is the next pass)**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors.

- [ ] **Step 4: Commit**

```bash
git add ClinicHub.Domain/Enums/NotificationType.cs ClinicHub.Infrastructure/Services/NotificationBuilderService.cs
git commit -m "feat(notifications): add AppointmentNoShow type and push template"
```

### Task 4 (continued): NoShowJob guard + notify

- [ ] **Step 5: Expose the grace period for reuse**

Edit `NoShowJob.cs` line 11 — change `private` to `internal`:

```csharp
internal static readonly TimeSpan GracePeriod = TimeSpan.FromMinutes(30);
```

Task 5's sweeper references `NoShowJob.GracePeriod` (DRY — one definition of "30 minutes").

- [ ] **Step 6: Skip checked-in patients and notify all three parties**

Replace the whole `MarkNoShowAsync` method (lines 22-38) with:

```csharp
public async Task MarkNoShowAsync(Guid appointmentId, CancellationToken cancellationToken)
{
    await using var scope = _serviceProvider.CreateAsyncScope();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var fcmService = scope.ServiceProvider.GetRequiredService<IFcmService>();

    var appointment = await unitOfWork.AppointmentRepository
        .GetFirstWithIncluding(a => a.Id == appointmentId, a => a.Clinic!, a => a.Doctor)
        .FirstOrDefaultAsync(cancellationToken);
    if (appointment is null)
        return;
    if (appointment.Status is not (AppointmentStatus.Accepted or AppointmentStatus.Confirmed))
        return;
    if (appointment.CheckedInAt.HasValue)
        return;

    var deadline = appointment.AppointmentDate.Add(appointment.EndTime).Add(GracePeriod);
    if (DateTime.Now < deadline)
        return;

    appointment.MarkNoShow();
    await unitOfWork.SaveChangesAsync();

    try
    {
        var parameters = new Dictionary<string, object>
        {
            ["patientName"] = appointment.PatientFullName,
            ["clinicName"] = appointment.Clinic?.Name ?? "",
            ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
            ["time"] = $"{appointment.StartTime:hh\\:mm} - {appointment.EndTime:hh\\:mm}",
            ["appointmentId"] = appointment.Id.ToString()
        };

        var recipients = new HashSet<Guid> { appointment.BookedByUserId };
        recipients.Add(appointment.Doctor.UserId);
        if (appointment.Clinic?.ClinicAdminId.HasValue == true)
            recipients.Add(appointment.Clinic.ClinicAdminId.Value);

        foreach (var userId in recipients)
            await fcmService.SendToUserAsync(userId, NotificationType.AppointmentNoShow, parameters);

        await unitOfWork.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to send no-show notifications for appointment {AppointmentId}.", appointmentId);
    }

    _logger.LogInformation("Appointment {AppointmentId} marked as no-show.", appointmentId);
}
```

Notes: `GetFirstWithIncluding` is the same loader the cancel handler uses (verify the `Doctor` include overload compiles — it takes `params Expression<Func<...>>[]`; two includes are used in `AppointmentAcceptanceService:139-140`). Marking commits FIRST so a push failure never un-voids the appointment; the second save persists only notification rows. `IFcmService` resolves from the job scope exactly like `CancellationWindowJob.cs:26` does. `Doctor` is non-nullable on the entity (`Appointment.cs:13`), so no null check needed; `ClinicAdminId` is `Guid?`, hence the `HasValue` check. Add usings: `ClinicHub.Application.Common.Interfaces` (for `IFcmService`), `Microsoft.EntityFrameworkCore` (for `FirstOrDefaultAsync`).

- [ ] **Step 7: Build**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors.

- [ ] **Step 8: Commit**

```bash
git add ClinicHub.Infrastructure/Services/BackgroundJobs/NoShowJob.cs
git commit -m "feat(appointments): no-show job respects check-in and notifies all parties"
```

### Task 5: Hourly no-show safety-net sweep

**Files:**
- Create: `ClinicHub.Infrastructure/Services/BackgroundJobs/NoShowSweepJob.cs`
- Modify: `ClinicHub.Infrastructure/DependencyInjection.cs:107`
- Modify: `ClinicHub.API/Program.cs:329-332`

Why a sweeper when per-entity jobs exist: `UpdateAppointment` reschedules without re-arming the one-off job (no `ScheduleNoShowMarkingAsync` call there), and lost Hangfire storage wipes one-off jobs silently. The codebase's own comment at `Program.cs:311` blesses this shape ("Recurring jobs — safety-net sweeps"). Guards are idempotent, so overlap with one-off jobs is harmless.

- [ ] **Step 1: Write the sweeper skeleton**

Create `NoShowSweepJob.cs` with namespace, usings, ctor, and an empty `SweepAsync` body:

```csharp
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Infrastructure.Services.BackgroundJobs;

public class NoShowSweepJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NoShowSweepJob> _logger;

    public NoShowSweepJob(IServiceProvider serviceProvider, ILogger<NoShowSweepJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SweepAsync(CancellationToken cancellationToken)
    {
        // BODY NEXT STEP
    }
}
```

- [ ] **Step 2: Implement the sweep body**

Replace `// BODY NEXT STEP` with:

```csharp
await using var scope = _serviceProvider.CreateAsyncScope();
var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
var fcmService = scope.ServiceProvider.GetRequiredService<IFcmService>();
var now = DateTime.Now;

var candidates = await unitOfWork.AppointmentRepository
    .GetAllAsync(a => (a.Status == AppointmentStatus.Accepted || a.Status == AppointmentStatus.Confirmed)
        && !a.CheckedInAt.HasValue
        && a.AppointmentDate <= now.Date)
    .Include(a => a.Clinic)
    .Include(a => a.Doctor)
    .ToListAsync(cancellationToken);

foreach (var appointment in candidates)
{
    var deadline = appointment.AppointmentDate.Add(appointment.EndTime).Add(NoShowJob.GracePeriod);
    if (now < deadline)
        continue;

    appointment.MarkNoShow();

    var parameters = new Dictionary<string, object>
    {
        ["patientName"] = appointment.PatientFullName,
        ["clinicName"] = appointment.Clinic?.Name ?? "",
        ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
        ["time"] = $"{appointment.StartTime:hh\\:mm} - {appointment.EndTime:hh\\:mm}",
        ["appointmentId"] = appointment.Id.ToString()
    };

    var recipients = new HashSet<Guid> { appointment.BookedByUserId, appointment.Doctor.UserId };
    if (appointment.Clinic?.ClinicAdminId.HasValue == true)
        recipients.Add(appointment.Clinic.ClinicAdminId.Value);

    foreach (var userId in recipients)
    {
        try
        {
            await fcmService.SendToUserAsync(userId, NotificationType.AppointmentNoShow, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send no-show notification to user {UserId} for appointment {AppointmentId}.", userId, appointment.Id);
        }
    }

    _logger.LogInformation("Appointment {AppointmentId} marked as no-show by sweep.", appointment.Id);
}

await unitOfWork.SaveChangesAsync();
```

Notes: needs `using ClinicHub.Domain.Enums;` (AppointmentStatus, NotificationType) — add to the skeleton usings. `GetAllAsync` returns `IQueryable` (Task 4's `AbandonedPaymentJob:29-33` chains `.ToListAsync` on it — same shape). `AppointmentDate <= now.Date` pre-filters in SQL; the exact deadline check runs in memory (avoids `Add(TimeSpan)` translation risk on Npgsql). One commit for the whole sweep (mirrors `AbandonedPaymentJob:45`).

- [ ] **Step 3: Register the job for DI**

Edit `DependencyInjection.cs` — after line 107 (`AddScoped<...NoShowJob>()`) add:

```csharp
services.AddScoped<ClinicHub.Infrastructure.Services.BackgroundJobs.NoShowSweepJob>();
```

- [ ] **Step 4: Register the hourly recurrence**

Edit `Program.cs` — after the `clinic-working-hours-validation` block (lines 331-332) add:

```csharp
RecurringJob.AddOrUpdate<NoShowSweepJob>("noshow-sweep",
    job => job.SweepAsync(CancellationToken.None), Cron.Hourly);
```

No new using needed (sibling job classes are referenced unqualified — verify by build).

- [ ] **Step 5: Build**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors.

- [ ] **Step 6: Commit**

```bash
git add ClinicHub.Infrastructure/Services/BackgroundJobs/NoShowSweepJob.cs ClinicHub.Infrastructure/DependencyInjection.cs ClinicHub.API/Program.cs
git commit -m "feat(appointments): hourly no-show safety-net sweep"
```

### Task 6: Reschedule resets check-in + end-to-end verification

**Files:**
- Modify: `ClinicHub.Domain/Entities/Appointment.cs:113-125` (`Update`)
- Test: build + Hangfire dashboard + Scalar (no test projects exist in this repo — compiler + dashboard + API checks are the TDD substitute, same as prior plans)

A checked-in patient who reschedules to a new slot must be treated as not-present for the NEW slot. Otherwise the sweeper skips them forever.

- [ ] **Step 1: Clear `CheckedInAt` when the slot changes**

Replace the `Update` method body (keep the signature):

```csharp
public void Update(
    DateTime appointmentDate,
    TimeSpan startTime,
    TimeSpan endTime,
    string complaint,
    string? chronicDiseases)
{
    var normalizedDate = new DateTime(appointmentDate.Year, appointmentDate.Month, appointmentDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
    if (AppointmentDate != normalizedDate || StartTime != startTime || EndTime != endTime)
        CheckedInAt = null;
    AppointmentDate = normalizedDate;
    StartTime = startTime;
    EndTime = endTime;
    Complaint = complaint;
    ChronicDiseases = chronicDiseases;
}
```

Complaint-only edits (same slot) keep the stamp — only a real slot move clears it. (`DateTime ==` compares ticks; both sides are midnight-Unspecified.)

- [ ] **Step 2: Build**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors.

- [ ] **Step 3: Commit**

```bash
git add ClinicHub.Domain/Entities/Appointment.cs
git commit -m "feat(appointments): rescheduling clears check-in stamp"
```

- [ ] **Step 4: Verify migrations + recurring job registration**

Run: `dotnet ef migrations list --project ClinicHub.Persistence --startup-project ClinicHub.API`
Expected: `AddCheckedInAtToAppointment` listed (applied or pending per environment).

Run the API (`dotnet run --project ClinicHub.API --launch-profile "ClinicHub.API"`), open `/hangfire`, section Recurring Jobs.
Expected: a `noshow-sweep` row with schedule `Hourly` and a successful first run.

- [ ] **Step 5: Verify manual-cancel clinic notice (Scalar `/scalar/v1`)**

As a patient, cancel an appointment with reason `test`. As the clinic admin, call `GET` `ApiRoutes.Notifications.GetAllPagginated` (`.../notifications/pagginated`).
Expected: a new `AppointmentCancellation` row whose `reason` starts with `Patient {FullName} cancelled: test`. The patient sees their own row with the raw reason.

- [ ] **Step 6: Verify auto no-show end-to-end (test DB only)**

On a TEST database, pick an `Accepted` appointment and backdate it past its grace window:

```sql
UPDATE "Appointments" SET "AppointmentDate" = CURRENT_DATE - INTERVAL '2 days', "EndTime" = '08:00:00' WHERE "Id" = '<appointment-guid>';
```

In `/hangfire` → Recurring Jobs → `noshow-sweep` → Trigger Now. Then:
1. `GET` appointment details → `Status` is `NoShow` (5).
2. Admin inbox (`.../notifications/pagginated`) shows an `AppointmentNoShow` row with the patient name, date, and time.
3. Repeat the trigger → nothing changes (idempotent: second run skips non-Accepted/Confirmed rows).
4. Check-in path: repeat with an appointment the staff checked in first → trigger → status stays `Confirmed` (proves the Task 1 guard).

- [ ] **Step 7: Final commit if verification touched code (usually empty)**

```bash
git status --short
```

Expected: clean tree. If verification exposed a bug, fix it as a new commit — do not amend.

---

## Self-Review

1. **Spec coverage:** "notification of manual cancel" → Task 3 (clinic side; patient side already existed). "Patient not present → grace 30 min → auto-cancel via Hangfire" → Task 4 (one-off job + all-party notify) + Task 5 (hourly sweeper covering reschedules/lost jobs). "Ensure presence correctly" → Task 1 (`CheckedInAt`), Task 6 (reschedule reset). Every clause has a task.
2. **Placeholder scan:** no TBD/TODO/"similar to"/"appropriate handling". Every code step shows the full block; every command has expected output. Cross-task references repeat the code instead of pointing.
3. **Type consistency:** `CheckedInAt` is `DateTime?` in entity, config, and migration. `AppointmentNoShow = 21` used identically in enum, builder, both jobs. `NoShowJob.GracePeriod` (`internal static`, `TimeSpan`) referenced by the sweeper — same symbol, same type. Recipient sets are `HashSet<Guid>` in all three notify sites. `roleLookup`-style dicts are not reused here — the sweep builds its own per-run.
4. **Blast radius double-check:** `CheckIn()` callers (`CheckInPatient`, `RegisterWalkInPatient`) both mean physical presence — stamping is correct for both. `Update()` callers get reset-on-slot-move only. `AppointmentCancellation` template text unchanged — existing patient/doctor-reject flows render byte-identically.
