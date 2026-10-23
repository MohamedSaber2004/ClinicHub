# Revenue Fee-Split Display Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Clinic notifications and clinic revenue show the patient-paid total together with the superadmin platform-fee cut (amount + percent + clinic net), and superadmin views show per-appointment total + fee with matching aggregates.

**Architecture:** `Payment.Amount` stays the single gross source of truth (no schema change, no migration). A new pure helper backs the fee out of the gross at read time (`net = Round(gross / (1 + percent/100))`, `fee = gross - net`, reusing `AppointmentPricingCalculator` rounding), and every revenue surface adds fee/net fields next to its unchanged gross totals.

**Tech Stack:** .NET 10, MediatR CQRS, EF Core + Npgsql, FCM via `IFcmService.SendToUserAsync` (Arabic templates hardcoded in `NotificationBuilderService`).

---

## Scope Check

One plan (not two): clinic notices, clinic revenue, and superadmin aggregates share one core (the gross→fee/net split + the FCM/DTO pattern). Each task below is independently buildable, committable, and verifiable via Scalar. No new API endpoints, no controller/route changes, no frontend changes (all DTO/notification additions are additive — old clients ignore unknown fields).

## Key Decisions (read before coding)

1. **Gross totals never change.** Every existing number (`TodayIncome`, `Revenue`, `totalRevenue`, `Amount`, …) keeps its exact value. Fee/net are NEW fields beside them. Zero regression risk for existing clients.
2. **Split is derived at read time from the CURRENT global percent** (`PlatformSetting.AppointmentFeePercent`, single row, via the existing `GetPlatformFeePercentAsync` snippet). No `Payment` schema change, no migration, no backfill. Consequence: if superadmin later changes the percent, displayed splits for old payments use the new percent (gross totals are always exact; only the split line moves). This matches how the codebase already reads the percent live at accept/pay time.
3. **Rounding rule (exact, no drift):** `net = Round(gross / (1 + p/100), 2, AwayFromZero)`, `fee = gross - net`. Defining fee as the remainder guarantees `net + fee == gross` to the halala. Forward check: 200 fee-base at 10% → patient pays 220; back-out gives net 200.00, fee 20.00.
4. **Per-row split, then sum.** Trend buckets, dashboard periods, and stats all split each payment row and sum the parts (never split an already-summed gross). All endpoints reconcile with each other.
5. **Fee applies to Appointment payments only.** Subscription/Ads rows and aggregates keep fee 0 and are otherwise untouched.
6. **No `messages.{en,ar}.json` changes.** Push templates are hardcoded Arabic in `NotificationBuilderService`; the API `localizer` path is untouched.
7. **No test project.** This repo has zero test projects (per `AGENTS.md`); verification is `dotnet build` + Scalar/API checks, same as the prior notifications plan. No new test infrastructure (YAGNI).
8. **Out of scope (deliberate):** `GetClinicOperationalReport.PeriodRevenue`, `GetClinicAdvancedReport` revenues, the patient-facing `PaymentConfirmation` notice, subscription/ads aggregates. They keep showing gross, unchanged. A follow-up plan can extend the split there using this plan's helper.

## File Structure Map

- Create: `ClinicHub.Application/Common/AppointmentRevenueSplitter.cs` — pure split helper (next to `AppointmentPricingCalculator.cs`).
- Modify: `ClinicHub.Application/Features/Payment/Commands/ConfirmPaymentWebhook/ConfirmPaymentWebhookCommandHandler.cs:148-182` — compute split, add fee params to both notices, add `GetPlatformFeePercentAsync`.
- Modify: `ClinicHub.Infrastructure/Services/NotificationBuilderService.cs:227-251` — `PaymentReceived` + `RevenueIncreased` templates show fee/percent/net.
- Modify: `ClinicHub.Application/Features/Admin/DTOs/AdminDashboardGraphDtos.cs:3-8` — `RevenueTrendPointDto` gains `PlatformFees`, `NetRevenue` (shared by clinic + admin trends).
- Modify: `ClinicHub.Application/Features/Clinics/DTOs/ClinicDashboardStatsDto.cs` — 8 new income-split fields.
- Modify: `ClinicHub.Application/Features/Clinics/Queries/GetClinicDashboardStats/GetClinicDashboardStatsQueryHandler.cs:58-88` — per-row split per period (add `using ClinicHub.Domain.Entities;`).
- Modify: `ClinicHub.Application/Features/Clinics/Queries/GetClinicRevenueTrend/GetClinicRevenueTrendQueryHandler.cs:33-60` — per-row split per bucket (add `using ClinicHub.Domain.Entities;`).
- Modify: `ClinicHub.Application/Features/Admin/Queries/GetRevenueTrend/GetRevenueTrendQueryHandler.cs:26-52` — same for the superadmin trend (no new using needed).
- Modify: `ClinicHub.Application/Features/AdminPayments/DTOs/AdminPaymentStatsDto.cs` — gains `AppointmentsPlatformFees`, `AppointmentsNetRevenue`.
- Modify: `ClinicHub.Application/Features/AdminPayments/Queries/GetAdminPaymentStats/GetAdminPaymentStatsQueryHandler.cs:41-70` — fee/net aggregates + `GetPlatformFeePercentAsync`.
- Modify: `ClinicHub.Application/Features/AdminPayments/DTOs/AdminPaymentDto.cs` — gains `PlatformFee`, `ClinicNetAmount`.
- Modify: `ClinicHub.Application/Features/AdminPayments/Queries/GetAdminPayments/GetAdminPaymentsQueryHandler.cs:53-82` — per-row split mapping + `GetPlatformFeePercentAsync`.

---

### Task 1: Shared revenue-split helper

**Files:**
- Create: `ClinicHub.Application/Common/AppointmentRevenueSplitter.cs`

Pure static logic, no DI, no DB. Lives next to `AppointmentPricingCalculator.cs` and reuses its rounding convention.

- [ ] **Step 1: Create the helper file**

Create `ClinicHub.Application/Common/AppointmentRevenueSplitter.cs` with this exact content:

```csharp
namespace ClinicHub.Application.Common
{
    /// <summary>
    /// Backs the platform-fee split out of a patient-paid gross total.
    /// Fee is defined as gross - net so net + fee always equals gross.
    /// </summary>
    public sealed record AppointmentRevenueSplit(decimal Total, decimal PlatformFee, decimal ClinicNet);

    public static class AppointmentRevenueSplitter
    {
        public static AppointmentRevenueSplit Split(decimal grossTotal, decimal percent)
        {
            if (percent <= 0)
                return new AppointmentRevenueSplit(grossTotal, 0m, grossTotal);
            var net = Math.Round(grossTotal / (1m + percent / 100m), 2, MidpointRounding.AwayFromZero);
            return new AppointmentRevenueSplit(grossTotal, grossTotal - net, net);
        }

        public static (decimal Fees, decimal Net) SumSplits(IEnumerable<decimal> grossTotals, decimal percent)
        {
            decimal fees = 0m, net = 0m;
            foreach (var gross in grossTotals)
            {
                var split = Split(gross, percent);
                fees += split.PlatformFee;
                net += split.ClinicNet;
            }
            return (fees, net);
        }
    }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors. (Helper is unused so far — no behavior change.)

- [ ] **Step 3: Commit**

```bash
git add ClinicHub.Application/Common/AppointmentRevenueSplitter.cs
git commit -m "feat(revenue): add gross-to-fee-net split helper"
```

---

### Task 2: Fee breakdown in payment notifications

**Files:**
- Modify: `ClinicHub.Application/Features/Payment/Commands/ConfirmPaymentWebhook/ConfirmPaymentWebhookCommandHandler.cs:148-182`
- Modify: `ClinicHub.Infrastructure/Services/NotificationBuilderService.cs:227-251`

Only the webhook sends `PaymentReceived`/`RevenueIncreased` (`VerifyBookingPayment` sends neither — verified). Gross keys (`amount`, `totalRevenue`) keep exact values; fee keys are new.

- [ ] **Step 1: Split the clinic-owner notice + load the percent**

Replace the `NotifyClinicOwnerAndSuperAdminsAsync` method (lines 148-182) with:

```csharp
private async Task NotifyClinicOwnerAndSuperAdminsAsync(Appointment appointment, ClinicHub.Domain.Entities.Payment payment)
{
    var percent = await GetPlatformFeePercentAsync(CancellationToken.None);
    var split = AppointmentRevenueSplitter.Split(payment.Amount, percent);
    var amount = $"{payment.Amount:N2} EGP";

    if (appointment.Clinic?.ClinicAdminId.HasValue == true)
    {
        await _fcmService.SendToUserAsync(appointment.Clinic.ClinicAdminId.Value, NotificationType.PaymentReceived, new()
        {
            ["amount"] = amount,
            ["patientName"] = appointment.PatientFullName ?? "",
            ["clinicName"] = appointment.Clinic.Name,
            ["appointmentId"] = appointment.Id.ToString(),
            ["platformFee"] = $"{split.PlatformFee:N2} EGP",
            ["feePercent"] = $"{percent:N2}%",
            ["netAmount"] = $"{split.ClinicNet:N2} EGP"
        });
    }

    // The current payment was only marked Paid in memory and is committed later by the
    // handler's SaveChangesAsync, so the DB sums below do not include it yet. Add its
    // split manually so the notification reports the totals AFTER this deposit.
    var paidAmounts = await _unitOfWork.PaymentRepository
        .GetAllAsync(p => p.Status == PaymentStatus.Paid && p.Type == PaymentType.Appointment)
        .Select(p => p.Amount)
        .ToListAsync();
    paidAmounts.Add(payment.Amount);
    var totalRevenue = paidAmounts.Sum();
    var (totalFees, totalNet) = AppointmentRevenueSplitter.SumSplits(paidAmounts, percent);

    var superAdmins = await _userManager.GetUsersInRoleAsync(UserType.SuperAdmin.ToString());
    foreach (var admin in superAdmins.Where(a => !a.IsDeleted))
    {
        await _fcmService.SendToUserAsync(admin.Id, NotificationType.RevenueIncreased, new()
        {
            ["amount"] = amount,
            ["clinicName"] = appointment.Clinic?.Name ?? "",
            ["totalRevenue"] = $"{totalRevenue:N2} EGP",
            ["appointmentId"] = appointment.Id.ToString(),
            ["totalPlatformFees"] = $"{totalFees:N2} EGP",
            ["totalNetRevenue"] = $"{totalNet:N2} EGP"
        });
    }
}

private async Task<decimal> GetPlatformFeePercentAsync(CancellationToken cancellationToken)
{
    var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
        .GetAllAsync(s => !s.IsDeleted)
        .OrderBy(s => s.CreatedAt)
        .FirstOrDefaultAsync(cancellationToken);
    return setting?.AppointmentFeePercent ?? 0m;
}
```

Notes: `AppointmentRevenueSplitter` needs `using ClinicHub.Application.Common;` at the top (same assembly, new namespace import). `PlatformSetting` resolves via the existing `using ClinicHub.Domain.Entities;` (line 2). `ToListAsync()` without a token matches the existing style at line 167-169. `CancellationToken.None` is correct here — this method has no token in scope and runs inside the webhook's non-critical side-effect block.

- [ ] **Step 2: Show fee/percent/net in both push templates**

Edit `NotificationBuilderService.cs` — replace line 229 (the `body = …` assignment only; leave every `data[…]` line in place) with:

```csharp
body = $"تم استلام دفعة بقيمة {amount} (شامل رسوم المنصة {GetParam(parameters, "platformFee")} بنسبة {GetParam(parameters, "feePercent")}) لحجز {GetParam(parameters, "patientName")} في عيادتك {clinicName} — صافي العيادة {GetParam(parameters, "netAmount")}";
data["platformFee"] = GetParam(parameters, "platformFee");
data["feePercent"] = GetParam(parameters, "feePercent");
data["netAmount"] = GetParam(parameters, "netAmount");
```

Keep the existing `data["amount"]`, `data["patientName"]`, `data["clinicName"]`, `data["appointmentId"]` lines untouched. Replace the `RevenueIncreased` case body (line 242) with:

```csharp
body = $"تم دفع {amount} لحجز في عيادة {clinicName} — إجمالي الإيرادات الآن {GetParam(parameters, "totalRevenue")} (منها رسوم المنصة {GetParam(parameters, "totalPlatformFees")} وصافي العيادات {GetParam(parameters, "totalNetRevenue")})";
data["totalPlatformFees"] = GetParam(parameters, "totalPlatformFees");
data["totalNetRevenue"] = GetParam(parameters, "totalNetRevenue");
```

Keep the existing `data["amount"]`, `data["clinicName"]`, `data["totalRevenue"]`, `data["appointmentId"]` lines untouched. Old app versions that only read `amount`/`totalRevenue` render the new body text fine — additive change.

- [ ] **Step 3: Build**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors.

- [ ] **Step 4: Commit**

```bash
git add ClinicHub.Application/Features/Payment/Commands/ConfirmPaymentWebhook/ConfirmPaymentWebhookCommandHandler.cs ClinicHub.Infrastructure/Services/NotificationBuilderService.cs
git commit -m "feat(notifications): show platform fee split in payment and revenue notices"
```

---

### Task 3: Fee/net on the clinic revenue endpoints

**Files:**
- Modify: `ClinicHub.Application/Features/Admin/DTOs/AdminDashboardGraphDtos.cs:3-8`
- Modify: `ClinicHub.Application/Features/Clinics/DTOs/ClinicDashboardStatsDto.cs`
- Modify: `ClinicHub.Application/Features/Clinics/Queries/GetClinicDashboardStats/GetClinicDashboardStatsQueryHandler.cs:1-88`
- Modify: `ClinicHub.Application/Features/Clinics/Queries/GetClinicRevenueTrend/GetClinicRevenueTrendQueryHandler.cs:1-60`

`RevenueTrendPointDto` is shared by the clinic trend AND the superadmin trend — one DTO edit serves both (Task 4 fills the admin handler). Gross fields stay byte-identical.

- [ ] **Step 1: Extend the shared trend DTO**

Edit `AdminDashboardGraphDtos.cs` — replace the `RevenueTrendPointDto` class with:

```csharp
public class RevenueTrendPointDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int PaymentsCount { get; set; }
    public decimal PlatformFees { get; set; }
    public decimal NetRevenue { get; set; }
}
```

- [ ] **Step 2: Extend the clinic dashboard DTO**

Edit `ClinicDashboardStatsDto.cs` — add 8 properties (keep the 9 existing ones untouched):

```csharp
public decimal TodayPlatformFees { get; set; }
public decimal TodayNetIncome { get; set; }
public decimal WeeklyPlatformFees { get; set; }
public decimal WeeklyNetIncome { get; set; }
public decimal MonthlyPlatformFees { get; set; }
public decimal MonthlyNetIncome { get; set; }
public decimal YearlyPlatformFees { get; set; }
public decimal YearlyNetIncome { get; set; }
```

- [ ] **Step 3: Fill fee/net in the clinic revenue trend**

Edit `GetClinicRevenueTrendQueryHandler.cs`: add `using ClinicHub.Application.Common;` and `using ClinicHub.Domain.Entities;` to the usings. Replace lines 33-60 (rows query + grouping + return) with:

```csharp
var percent = await GetPlatformFeePercentAsync(cancellationToken);

var rows = await _unitOfWork.GetRepository<PaymentEntity, Guid>()
    .GetAllAsync(p => p.ClinicId == clinicId
        && p.Type == PaymentType.Appointment
        && p.Status == PaymentStatus.Paid
        && p.PaidAt != null
        && p.PaidAt >= fromDate
        && p.PaidAt < toDate)
    .Select(p => new { p.PaidAt, p.Amount })
    .ToListAsync(cancellationToken);

var grouped = rows
    .GroupBy(r => GraphPeriodHelper.BucketStart(r.PaidAt!.Value, granularity))
    .ToDictionary(
        g => g.Key,
        g => (Revenue: g.Sum(x => x.Amount), Count: g.Count(),
              Split: AppointmentRevenueSplitter.SumSplits(g.Select(x => x.Amount), percent)));

return GraphPeriodHelper.BuildBuckets(fromDate, toDate, granularity)
    .Select(b =>
    {
        grouped.TryGetValue(b, out var v);
        return new RevenueTrendPointDto
        {
            Period = GraphPeriodHelper.FormatBucket(b, granularity),
            Revenue = v.Revenue,
            PaymentsCount = v.Count,
            PlatformFees = v.Split.Fees,
            NetRevenue = v.Split.Net
        };
    })
    .ToList();
```

Append this private method at the end of the handler class (before the closing braces):

```csharp
private async Task<decimal> GetPlatformFeePercentAsync(CancellationToken cancellationToken)
{
    var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
        .GetAllAsync(s => !s.IsDeleted)
        .OrderBy(s => s.CreatedAt)
        .FirstOrDefaultAsync(cancellationToken);
    return setting?.AppointmentFeePercent ?? 0m;
}
```

`PaymentEntity` is already aliased at the top of this file (line 8) — keep using it.

- [ ] **Step 4: Fill fee/net in the clinic dashboard stats**

Edit `GetClinicDashboardStatsQueryHandler.cs`: add `using ClinicHub.Application.Common;` and `using ClinicHub.Domain.Entities;` to the usings. After line 40 (`paymentsQuery` definition), insert:

```csharp
var percent = await GetPlatformFeePercentAsync(cancellationToken);
```

Replace the four income blocks (lines 58-72) with amount-list loads plus split-sums:

```csharp
var todayAmounts = await paymentsQuery
    .Where(p => p.PaidAt >= todayStart && p.PaidAt < todayEnd)
    .Select(p => p.Amount)
    .ToListAsync(cancellationToken);
var weeklyAmounts = await paymentsQuery
    .Where(p => p.PaidAt >= weekStart && p.PaidAt < todayEnd)
    .Select(p => p.Amount)
    .ToListAsync(cancellationToken);
var monthlyAmounts = await paymentsQuery
    .Where(p => p.PaidAt >= monthStart && p.PaidAt < todayEnd)
    .Select(p => p.Amount)
    .ToListAsync(cancellationToken);
var yearlyAmounts = await paymentsQuery
    .Where(p => p.PaidAt >= yearStart && p.PaidAt < todayEnd)
    .Select(p => p.Amount)
    .ToListAsync(cancellationToken);

var todayIncome = todayAmounts.Sum();
var (todayFees, todayNet) = AppointmentRevenueSplitter.SumSplits(todayAmounts, percent);
var weeklyIncome = weeklyAmounts.Sum();
var (weeklyFees, weeklyNet) = AppointmentRevenueSplitter.SumSplits(weeklyAmounts, percent);
var monthlyIncome = monthlyAmounts.Sum();
var (monthlyFees, monthlyNet) = AppointmentRevenueSplitter.SumSplits(monthlyAmounts, percent);
var yearlyIncome = yearlyAmounts.Sum();
var (yearlyFees, yearlyNet) = AppointmentRevenueSplitter.SumSplits(yearlyAmounts, percent);
```

Extend the return block (lines 77-88) with the 8 new assignments (`TodayPlatformFees = todayFees, TodayNetIncome = todayNet, …`). Append the same `GetPlatformFeePercentAsync` private method from Step 3 to this handler class.

- [ ] **Step 5: Build**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors.

- [ ] **Step 6: Commit**

```bash
git add ClinicHub.Application/Features/Admin/DTOs/AdminDashboardGraphDtos.cs ClinicHub.Application/Features/Clinics/DTOs/ClinicDashboardStatsDto.cs ClinicHub.Application/Features/Clinics/Queries/GetClinicDashboardStats/GetClinicDashboardStatsQueryHandler.cs ClinicHub.Application/Features/Clinics/Queries/GetClinicRevenueTrend/GetClinicRevenueTrendQueryHandler.cs
git commit -m "feat(revenue): expose platform fee split on clinic revenue endpoints"
```

---

### Task 4: Fee/net on the superadmin aggregates

**Files:**
- Modify: `ClinicHub.Application/Features/Admin/Queries/GetRevenueTrend/GetRevenueTrendQueryHandler.cs:21-53`
- Modify: `ClinicHub.Application/Features/AdminPayments/DTOs/AdminPaymentStatsDto.cs`
- Modify: `ClinicHub.Application/Features/AdminPayments/Queries/GetAdminPaymentStats/GetAdminPaymentStatsQueryHandler.cs:19-71`

The admin trend reuses `RevenueTrendPointDto` from Task 3 — no DTO work needed for it. The `RevenueIncreased` notice aggregate was already handled in Task 2.

- [ ] **Step 1: Fill fee/net in the superadmin revenue trend**

Edit `Admin/Queries/GetRevenueTrend/GetRevenueTrendQueryHandler.cs`: add `using ClinicHub.Application.Common;` and `using ClinicHub.Domain.Entities;` (`PlatformSetting` needs the latter; `PaymentEntity` alias already exists). Replace lines 26-51 (rows query + grouping + return) with:

```csharp
var percent = await GetPlatformFeePercentAsync(cancellationToken);

var rows = await _unitOfWork.GetRepository<PaymentEntity, Guid>()
    .GetAllAsync(p => p.Status == PaymentStatus.Paid
        && p.PaidAt != null
        && p.PaidAt >= fromDate
        && p.PaidAt < toDate)
    .Select(p => new { p.PaidAt, p.Amount, p.Type })
    .ToListAsync(cancellationToken);

var grouped = rows
    .GroupBy(r => GraphPeriodHelper.BucketStart(r.PaidAt!.Value, granularity))
    .ToDictionary(
        g => g.Key,
        g => (Revenue: g.Sum(x => x.Amount), Count: g.Count(),
              Split: AppointmentRevenueSplitter.SumSplits(
                  g.Where(x => x.Type == PaymentType.Appointment).Select(x => x.Amount), percent)));

return GraphPeriodHelper.BuildBuckets(fromDate, toDate, granularity)
    .Select(b =>
    {
        grouped.TryGetValue(b, out var v);
        return new RevenueTrendPointDto
        {
            Period = GraphPeriodHelper.FormatBucket(b, granularity),
            Revenue = v.Revenue,
            PaymentsCount = v.Count,
            PlatformFees = v.Split.Fees,
            NetRevenue = v.Split.Net
        };
    })
    .ToList();
```

Note the deliberate asymmetry: `Revenue`/`PaymentsCount` cover ALL paid types (unchanged behavior), while `PlatformFees`/`NetRevenue` cover Appointment payments only (Decision 5). Append this private method to this handler class:

```csharp
private async Task<decimal> GetPlatformFeePercentAsync(CancellationToken cancellationToken)
{
    var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
        .GetAllAsync(s => !s.IsDeleted)
        .OrderBy(s => s.CreatedAt)
        .FirstOrDefaultAsync(cancellationToken);
    return setting?.AppointmentFeePercent ?? 0m;
}
```

`PaymentType` resolves via the existing `using ClinicHub.Domain.Enums;` (line 4).

- [ ] **Step 2: Extend the superadmin stats DTO**

Edit `AdminPaymentStatsDto.cs` — append two properties:

```csharp
public decimal AppointmentsPlatformFees { get; set; }
public decimal AppointmentsNetRevenue { get; set; }
```

- [ ] **Step 3: Fill fee/net in the superadmin stats**

Edit `GetAdminPaymentStatsQueryHandler.cs`: add `using ClinicHub.Application.Common;` (`Entities` + `Enums` + EFCore usings already present). After line 43 (`appointmentsRevenue` computation), insert:

```csharp
var appointmentAmounts = await query
    .Where(p => p.Type == PaymentType.Appointment && p.Status == PaymentStatus.Paid)
    .Select(p => p.Amount)
    .ToListAsync(cancellationToken);
var percent = await GetPlatformFeePercentAsync(cancellationToken);
var (appointmentsFees, appointmentsNet) = AppointmentRevenueSplitter.SumSplits(appointmentAmounts, percent);
```

Extend the return block with `AppointmentsPlatformFees = appointmentsFees, AppointmentsNetRevenue = appointmentsNet`. Append this private method to the handler class:

```csharp
private async Task<decimal> GetPlatformFeePercentAsync(CancellationToken cancellationToken)
{
    var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
        .GetAllAsync(s => !s.IsDeleted)
        .OrderBy(s => s.CreatedAt)
        .FirstOrDefaultAsync(cancellationToken);
    return setting?.AppointmentFeePercent ?? 0m;
}
```

`query` is reassigned by filters above — the new `Where` chains on the filtered query, so date/type filters apply to the split exactly as they do to `appointmentsRevenue`.

- [ ] **Step 4: Build**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors.

- [ ] **Step 5: Commit**

```bash
git add ClinicHub.Application/Features/Admin/Queries/GetRevenueTrend/GetRevenueTrendQueryHandler.cs ClinicHub.Application/Features/AdminPayments/DTOs/AdminPaymentStatsDto.cs ClinicHub.Application/Features/AdminPayments/Queries/GetAdminPaymentStats/GetAdminPaymentStatsQueryHandler.cs
git commit -m "feat(revenue): expose platform fee split on superadmin aggregates"
```

---

### Task 5: Per-appointment total + fee rows for superadmin

**Files:**
- Modify: `ClinicHub.Application/Features/AdminPayments/DTOs/AdminPaymentDto.cs`
- Modify: `ClinicHub.Application/Features/AdminPayments/Queries/GetAdminPayments/GetAdminPaymentsQueryHandler.cs:20-83`

This is the "for each appointment" half of the spec: every payment row carries its own total + fee + net. Fee is 0 for non-Appointment rows (Decision 5).

- [ ] **Step 1: Extend the row DTO**

Edit `AdminPaymentDto.cs` — append two properties:

```csharp
public decimal PlatformFee { get; set; }
public decimal ClinicNetAmount { get; set; }
```

- [ ] **Step 2: Split each row in the handler**

Edit `GetAdminPaymentsQueryHandler.cs`: add `using ClinicHub.Application.Common;` (`Entities`/`Enums`/EFCore usings already present). After line 53 (`var payments = await query.ToListAsync(cancellationToken);`), insert:

```csharp
var percent = await GetPlatformFeePercentAsync(cancellationToken);
```

Replace the `.Select(p => new AdminPaymentDto { … })` projection (lines 67-79) with:

```csharp
.Select(p =>
{
    var split = p.Type == PaymentType.Appointment
        ? AppointmentRevenueSplitter.Split(p.Amount, percent)
        : new AppointmentRevenueSplit(p.Amount, 0m, p.Amount);
    return new AdminPaymentDto
    {
        Id = p.Id,
        Code = p.Code,
        Type = p.Type,
        Payer = ResolvePayer(p),
        Amount = p.Amount,
        Currency = p.Currency,
        Method = PaymentMethodMapper.ToEnum(p.PaymentMethod),
        Status = PaymentMethodMapper.ToUiStatus(p.Status),
        Date = p.CreatedAt,
        RefNumber = p.RefNumber,
        PlatformFee = split.PlatformFee,
        ClinicNetAmount = split.ClinicNet
    };
})
```

Append this private method to the handler class:

```csharp
private async Task<decimal> GetPlatformFeePercentAsync(CancellationToken cancellationToken)
{
    var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
        .GetAllAsync(s => !s.IsDeleted)
        .OrderBy(s => s.CreatedAt)
        .FirstOrDefaultAsync(cancellationToken);
    return setting?.AppointmentFeePercent ?? 0m;
}
```

`PaymentType`/`AppointmentRevenueSplit` resolve via existing + new usings. Pagination (`Skip`/`Take` on lines 64-66) is untouched — the split runs on the page only, one percent lookup per request.

- [ ] **Step 3: Build**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors.

- [ ] **Step 4: Commit**

```bash
git add ClinicHub.Application/Features/AdminPayments/DTOs/AdminPaymentDto.cs ClinicHub.Application/Features/AdminPayments/Queries/GetAdminPayments/GetAdminPaymentsQueryHandler.cs
git commit -m "feat(revenue): show per-appointment total and platform fee to superadmin"
```

---

### Task 6: End-to-end verification

**Files:**
- Test: build + Scalar `/scalar/v1` + notifications inbox (no test projects exist — compiler + API checks are the TDD substitute, same as prior plans)

Setup: confirm the live percent first — call the platform-settings get endpoint (or `SELECT "AppointmentFeePercent" FROM "PlatformSettings"`) and note it as P. All expected numbers below assume P = 10 and a 200-fee clinic (patient paid 220). Recompute with the helper rule (Decision 3) if P differs.

- [ ] **Step 1: Final build + migration check**

Run: `dotnet build ClinicHub.slnx --no-restore`
Expected: PASS, 0 Errors.

Run: `dotnet ef migrations list --project ClinicHub.Persistence --startup-project ClinicHub.API --context ClinicHubContext`
Expected: no new migration appears (this plan adds none — read-time derivation only). If a pending migration shows, stop — it is unrelated drift, do not apply it here.

- [ ] **Step 2: Verify clinic notification (Scalar + inbox)**

Run the API (`dotnet run --project ClinicHub.API --launch-profile "ClinicHub.API"`). Complete a test appointment payment through the webhook path (Paymob sandbox or the test verify flow), then as the clinic admin call the notifications paginated endpoint.
Expected: a new `PaymentReceived` row whose body reads `تم استلام دفعة بقيمة 220.00 EGP (شامل رسوم المنصة 20.00 EGP بنسبة 10.00%) … صافي العيادة 200.00 EGP`, with `data` keys `platformFee`, `feePercent`, `netAmount` present and `amount` still `220.00 EGP`.

- [ ] **Step 3: Verify superadmin notification + aggregates**

As a superadmin, check the inbox for the matching `RevenueIncreased` row.
Expected: body ends with `(منها رسوم المنصة 20.00 EGP وصافي العيادات 200.00 EGP)` (plus any pre-existing paid-appointment sums), `totalRevenue` still the gross sum.
Call the admin payment-stats endpoint.
Expected: `appointmentsRevenue` = gross sum (unchanged), `appointmentsPlatformFees` + `appointmentsNetRevenue` sum to it exactly.
Call the admin revenue-trend endpoint.
Expected: each bucket has `platformFees`/`netRevenue` that add up to `revenue`.

- [ ] **Step 4: Verify clinic revenue + per-appointment rows**

As the clinic user, call dashboard-stats and revenue-trend.
Expected: all `*Income`/`revenue` values identical to before the change; each period/bucket satisfies `platformFees + netRevenue == revenue` (e.g. 20.00 + 200.00 == 220.00).
As superadmin, call the admin payments list filtered to appointments.
Expected: every row shows `platformFee`/`clinicNetAmount` (20.00/200.00 in the example) with `amount` unchanged at 220.00.

- [ ] **Step 5: Verify out-of-scope surfaces are untouched**

Open the operational report and advanced report for the same clinic/period.
Expected: `periodRevenue`, `totalRevenue`, `averageAppointmentValue`, `revenueByDoctor` values identical to pre-change (gross, no split fields). Any difference means a task over-reached — revert it.

- [ ] **Step 6: Final commit if verification touched code (usually empty)**

```bash
git status --short
```

Expected: clean tree (apart from pre-existing unrelated files). If verification exposed a bug, fix it as a new commit — do not amend.

---

## Self-Review

1. **Spec coverage:** "clinic notifications … show the total with … superadmin fees" → Task 2 (fee/percent/net beside unchanged gross). "clinic … revenue" → Task 3 (dashboard stats + trend). "superadmin calculate all total + fees for each appointment" → Task 5 (per-row total + fee) + Task 4 (matching trend/stats aggregates) + Task 2 (`RevenueIncreased` totals). Every clause has a task.
2. **Placeholder scan:** no TBD/TODO/"similar to"/"appropriate handling". Every code step shows the full block; `GetPlatformFeePercentAsync` is repeated verbatim per task (matches the codebase's existing triplication); every command has expected output. Cross-task references repeat the code instead of pointing.
3. **Type consistency:** `AppointmentRevenueSplit(Total, PlatformFee, ClinicNet)` constructed positionally in Tasks 2 and 5 with the same order; `SumSplits` returns named tuple `(Fees, Net)` destructured identically in Tasks 2-4. DTO fields (`PlatformFees`/`NetRevenue`, `*PlatformFees`/`*NetIncome`, `PlatformFee`/`ClinicNetAmount`) match handler assignments task-by-task. `percent` is `decimal` everywhere, formatted `N2` only at the FCM boundary.
4. **Blast radius double-check:** gross values untouched in all 8 modified surfaces; reports/patient notice/subscription/ads explicitly excluded; no migration (nothing to apply at deploy); `RevenueTrendPointDto` is shared but both consumers are updated in Tasks 3-4 together; per-request percent lookups add one indexed single-row query (same pattern the codebase already uses 3×).
