# Subscription Pro-Rated Cancellation & Refund — Implementation Plan

> **For agentic workers:** use `subagent-driven-development` (recommended) or `executing-plans` to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A clinic owner on an online-paid plan can cancel at any time. The subscription ends immediately, the days already used are charged, and the unused remainder is refunded through Paymob to the original card/wallet. A superadmin can perform the same cancellation from the dashboard, optionally overriding the refund amount.

**Architecture:** Cancellation is committed first (subscription state + refund intent in one DB transaction), then the pro-rated amount is sent to Paymob outside that transaction. The refund is a first-class payment state (`RefundStatus`, `RefundAmount`, `RefundedAmount`), so an ambiguous Paymob outcome can never trigger a second refund. A pure calculator is shared by the quote endpoint, owner cancel, admin cancel, and the retry job — one formula everywhere.

**Tech Stack:** .NET 10, MediatR CQRS, EF Core (SQL Server), FluentValidation, Hangfire, FCM (`IFcmService`), Paymob legacy refund API (`POST /api/acceptance/void_refund/refund`), xUnit is **not** present in this repo.

---

## 1. Scope Check

One plan. It touches Domain, Persistence (one additive migration), Application handlers, Infrastructure (retry job + notification template), and three API endpoints. No frontend code. No new test project (repo has none, per `AGENTS.md`); verification is build + Scalar/SQL checks.

**Out of scope (deliberate):**

- Refunds for appointment or ads payments (existing flows keep working unchanged).
- Automatic renewals or installment-based subscriptions.
- A full accounting/ledger module. This plan only records refunded amounts on the payment.
- Frontend screens. The API returns everything the dashboard needs.

## 2. Product Policy

| Case | Result |
|---|---|
| Monthly 200 EGP, started Sep 1, cancelled Sep 3 | used 2/30 days → charge **13.33** → refund **186.67** |
| Cancel on the first day | minimum **1 day** is charged |
| Cancel on the last day | charge = full amount, refund **0** |
| Yearly plan | pro-rated over the real 365/366 days of the period |
| Subscription already expired (`EndDate <= now`) | not cancellable; no refund |
| Cash / admin-granted subscription | owner gets `400`; **only an admin** can cancel, with no automatic Paymob refund |
| Paymob refund returns a definite failure | cancellation **stands**; refund stays `Pending`, background retry runs, admins are alerted |
| Paymob call times out / throws (unknown outcome) | refund becomes `NeedsReview`, **never auto-retried** (prevents double refund) |
| Owner clicks cancel twice / two servers race | one succeeds; the other gets `400`/`409` — subscription is no longer `Active` |
| Admin override exceeds the paid amount | `400` |
| Subscription payment already fully refunded | `400` |

## 3. Verified Current State

| Area | Finding |
|---|---|
| Paymob partial refund | **Already supported** — `PaymobService.RefundTransactionAsync` sends `amount_cents` (`ClinicHub.Infrastructure/Services/Paymob/PaymobService.cs:351`) |
| Owner cancel | bare status change, no refund (`ClinicHub.Application/Features/Subscriptions/Commands/CancelMySubscription/CancelMySubscriptionCommandHandler.cs:35`) |
| Admin revoke | refunds **100%** (`.../RevokeSubscription/RevokeSubscriptionCommandHandler.cs:50`) and stores the refund id in `RefundReason` |
| `Payment` | no partial state, no refunded-amount field; `MarkAsRefunded` sets `Status = Refunded` and nulls `PaidAt`/`PaymentMethod` (`ClinicHub.Domain/Entities/Payment.cs:105`) |
| `Subscription` | no `CancelledAt`, no cancellation source, no refund amount (`ClinicHub.Domain/Entities/Subscription.cs`) |
| Concurrency | both `Payment` and `Subscription` already have `rowversion` (`Version`, `[Timestamp]` in `BaseEntity`); EF maps it as a concurrency token in the model snapshot |
| Refund dedupe | `PaymentRefundGate` serializes per payment in-process (`ClinicHub.Application/Common/PaymentRefundGate.cs:9`) |
| Retry job | refunds `payment.Amount` and only accepts `Paid` (`ClinicHub.Infrastructure/Services/BackgroundJobs/RefundRetryJob.cs:39,63`) — **would double-refund under partials, must change** |
| Revenue reports | filter only `PaymentStatus.Paid` (`GetAdminPaymentStatsQueryHandler.cs:38-59`, `GetRevenueTrendQueryHandler.cs:34`, `GetClinicsPaymentsSummaryQueryHandler.cs:40`) — a new status would silently erase revenue |
| Concurrency error handling | `DbUpdateConcurrencyException` is already translated by `ApiExceptionFilterAttribute` (`ClinicHub.API/Filters/ApiExceptionFilterAttribute.cs:16`) |
| Online-payment marker | online payments set `Payment.PaymobTransactionId` (numeric); manual/cash payments set only `TransactionId` via `MarkAsManuallyPaid` |

## 4. Key Decisions (read before coding)

1. **Only an online Paymob transaction can be self-cancelled.** The reliable test is a numeric `PaymobTransactionId`. Do **not** use `PaymentMethod` strings (free-text) or `TransactionId` (manual payments use it).
2. **Cancel first, refund second.** The cancellation and the refund intent are committed in one DB transaction. No money moves until that commit is durable. Paymob is called only afterwards.
3. **Never hold a DB transaction across a network call.** The Paymob call happens after `CommitAsync()`.
4. **The refund amount is frozen before calling Paymob.** It is stored in `Payment.RefundAmount` so a retry uses the exact same number, even if dates/plan change later.
5. **`Pending → Processing` is a persisted claim.** Combined with the `Payment.Version` rowversion, only one request can own a refund, even across multiple app instances.
6. **Ambiguous Paymob outcome = `NeedsReview`, not retry.** If the HTTP call times out or the process dies after sending it, we cannot know whether money moved. Retrying could refund twice; a human must reconcile.
7. **`PartiallyRefunded` is a real status and revenue must net it.** Reports treat it as `Amount - RefundedAmount`, and count it in refunded counts.
8. **The existing appointment refund path is untouched.** `RefundRetryJob` keeps its legacy full-refund branch for appointments and adds a separate `RefundStatus == Pending` branch for subscriptions.
9. **Append enum values only.** `PaymentStatus` and `NotificationType` are persisted/scaled as integers; new members go at the end.
10. **No test project.** Verification is `dotnet build` + Scalar + SQL checks, documented in section 12.

## 5. Money Calculation

One pure static helper. It is the single source of truth for every path.

```csharp
public sealed record RefundQuote(int TotalDays, int UsedDays, decimal ChargeAmount, decimal RefundAmount);

public static class SubscriptionRefundCalculator
{
    public static RefundQuote Calculate(
        decimal amountPaid,
        decimal alreadyRefunded,
        DateTime startDate,
        DateTime endDate,
        DateTime cancelledAt)
    {
        var totalDays = Math.Max(1, (endDate.Date - startDate.Date).Days);
        var usedDays = Math.Clamp((cancelledAt.Date - startDate.Date).Days, 1, totalDays);
        var basis = Math.Max(0m, amountPaid - alreadyRefunded);
        var charge = Math.Round(basis * usedDays / totalDays, 2, MidpointRounding.AwayFromZero);
        var refund = Math.Max(0m, Math.Round(basis - charge, 2, MidpointRounding.AwayFromZero));

        return new RefundQuote(totalDays, usedDays, charge, refund);
    }
}
```

Rules baked in:

- `totalDays` uses real calendar days (`StartDate` → `EndDate`), so a 31-day month charges `2/31`, and a year charges `2/365` (or `2/366`).
- `usedDays` has a floor of **1** (same-day cancellation still costs one day) and a ceiling of `totalDays`.
- Charge and refund are rounded to 2 decimals, half away from zero (matches existing money code).
- `basis` subtracts any previous refunds, so the cap is always the remaining paid amount.
- Cancelling on/after `EndDate` yields `usedDays == totalDays` and `refund == 0`.

Worked examples (expected values for verification):

| Plan | Amount | Period | Cancel | Used | Charge | Refund |
|---|---|---|---|---|---|---|
| Monthly | 200.00 | Sep 1 → Oct 1 (30d) | Sep 3 | 2 | 13.33 | 186.67 |
| Monthly | 200.00 | Oct 1 → Nov 1 (31d) | Oct 3 | 2 | 12.90 | 187.10 |
| Monthly | 200.00 | Sep 1 → Oct 1 (30d) | Sep 1 | 1 | 6.67 | 193.33 |
| Monthly | 200.00 | Sep 1 → Oct 1 (30d) | Oct 1 | 30 | 200.00 | 0.00 |
| Yearly | 2400.00 | Jan 1 → Jan 1 (365d) | Jan 3 | 2 | 13.15 | 2386.85 |

## 6. State Model

### 6.1 `PaymentStatus` (append only)

```csharp
public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3,
    Processing = 4,
    PartiallyRefunded = 5,
    RefundPending = 6,
    RefundFailed = 7
}
```

- `Status` describes the **money**: `Paid` → `PartiallyRefunded` or `Refunded` after Paymob confirms.
- A pending/failed refund does **not** change `Status`; it stays `Paid`.
- `RefundPending`/`RefundFailed` exist for filtering and dashboards but are set in tandem with `RefundStatus` to avoid ambiguity. If you prefer a single source of truth, use `RefundStatus` only and skip these two members; the plan works either way.

### 6.2 `PaymentRefundStatus` (new)

```csharp
public enum PaymentRefundStatus
{
    None = 0,
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    NeedsReview = 5
}
```

### 6.3 `SubscriptionCancellationSource` (new)

```csharp
public enum SubscriptionCancellationSource
{
    ClinicOwner = 0,
    Admin = 1,
    System = 2
}
```

### 6.4 `Payment` new fields

```csharp
public PaymentRefundStatus RefundStatus { get; private set; } = PaymentRefundStatus.None;
public decimal RefundAmount { get; private set; }          // approved amount to send
public decimal RefundedAmount { get; private set; }        // amount Paymob accepted
public string? RefundId { get; private set; }              // Paymob refund transaction id
public DateTime? RefundRequestedAt { get; private set; }
public int RefundAttemptCount { get; private set; }
public string? RefundFailureReason { get; private set; }
```

Domain methods:

```csharp
public void RequestRefund(decimal amount, string? reason)
{
    RefundStatus = PaymentRefundStatus.Pending;
    RefundAmount = amount;
    RefundAttemptCount = 0;
    RefundFailureReason = null;
    RefundRequestedAt = DateTime.Now;
    RefundReason = reason;
}

public bool ClaimRefund()
{
    if (RefundStatus != PaymentRefundStatus.Pending) return false;
    RefundStatus = PaymentRefundStatus.Processing;
    RefundAttemptCount++;
    return true;
}

public void CompleteRefund(decimal amount, string? refundId)
{
    RefundedAmount += amount;
    RefundId = refundId;
    RefundStatus = PaymentRefundStatus.Completed;
    RefundedAt = DateTime.Now;
    FailureReason = null;
    // Keep PaidAt and PaymentMethod intact for audit.
    Status = RefundedAmount >= Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
}

public void FailRefund(string? reason)
{
    RefundStatus = PaymentRefundStatus.Pending; // eligibility for retry
    RefundFailureReason = reason;
}

public void RequireRefundReview(string? reason)
{
    RefundStatus = PaymentRefundStatus.NeedsReview;
    RefundFailureReason = reason;
}
```

> `MarkAsRefunded` stays exactly as it is for the appointment/admin flows. Do not change its `PaidAt = null` behavior in this plan.

### 6.5 `Subscription` new fields

```csharp
public DateTime? CancelledAt { get; set; }
public SubscriptionCancellationSource? CancellationSource { get; set; }
public Guid? CancelledByUserId { get; set; }
public string? CancellationReason { get; set; }
public decimal RefundAmount { get; set; }
```

EF configuration:

- `Payment.RefundAmount`, `Payment.RefundedAmount` → `decimal(18,2)`, required, default `0`.
- `Payment.RefundId` → `nvarchar(100)`.
- `Payment.RefundFailureReason` → `nvarchar(500)`.
- `Subscription.CancellationReason` → `nvarchar(500)`.
- `Subscription.RefundAmount` → `decimal(18,2)`, required, default `0`.
- Index `Payments(RefundStatus)` to make the retry query cheap.
- No new `Version` configuration needed — `[Timestamp]` on `BaseEntity` already maps it.

## 7. Owner Cancellation Flow (online only)

**Endpoint:** `POST /api/v1/subscriptions/my/cancel` — `RoleAuthorize(ClinicOwner)`

1. **Resolve context** — `ICurrentUserService.CurrentClinicId` must exist; otherwise `400`.
2. **Load the active subscription** — `ClinicId == clinicId && Status == Active && EndDate > now`; otherwise `404 No active subscription found`.
3. **Load the linked payment** — `subscription.PaymentId`; must exist, `Type == Subscription`, `Status is Paid or PartiallyRefunded`.
4. **Online check** — `long.TryParse(payment.PaymobTransactionId, out _)` must be true. Otherwise:
   `400 Only online-paid subscriptions can be cancelled from the dashboard. Please contact support.`
5. **Compute the quote** — `SubscriptionRefundCalculator.Calculate(payment.Amount, payment.RefundedAmount, subscription.StartDate, subscription.EndDate, now)`.
6. **Commit cancellation + refund intent** inside `BeginTransactionAsync()`:
   - `subscription.Status = Cancelled`
   - `subscription.CancelledAt = now`, `CancellationSource = ClinicOwner`, `CancelledByUserId = currentUser`, `CancellationReason = reason`, `RefundAmount = quote.RefundAmount`
   - if `quote.RefundAmount > 0`: `payment.RequestRefund(quote.RefundAmount, "Pro-rated subscription cancellation")`
   - `SaveChangesAsync()` then `CommitAsync()`
   - `DbUpdateConcurrencyException` → let the global filter return `409` (double-click protection via `Subscription.Version`)
7. **If refund == 0**: skip Paymob, notify, return `refundStatus = None`.
8. **Call Paymob (outside the transaction)** via `ISubscriptionCancellationService.RefundAsync(payment.Id)`:
   - `PaymentRefundGate.RunAsync(paymentId, ...)`
   - re-read payment; if `RefundStatus != Pending` → another request owns it, return current state
   - `payment.ClaimRefund()` + `SaveChangesAsync()` — this is the durable claim; `Payment.Version` makes a concurrent claim fail
   - `await _paymobService.RefundTransactionAsync(payment.PaymobTransactionId!, payment.RefundAmount, ct)`
   - success → `payment.CompleteRefund(payment.RefundAmount, refund.RefundId ?? refund.Message)`, save
   - definite failure (`Success == false`) → `payment.FailRefund(refund.Message)`, save, `ScheduleRefundRetryAsync(payment.Id)`
   - exception/timeout → `payment.RequireRefundReview(ex.Message)`, save, notify admins, **do not retry**
9. **Respond** with `CancelSubscriptionResponseDto`.

> Owner cancellation of a subscription with a future `StartDate` is treated as day 1 used.

## 8. Admin Cancellation Flow

**Endpoint:** `POST /api/v1/admin/dashboard/subscriptions/{id}/revoke` — SuperAdmin (existing route)

Body:

```json
{
  "refundAmount": 186.67,
  "reason": "Clinic requested cancellation"
}
```

- `refundAmount` omitted/null → use the pro-rated quote.
- `refundAmount = 0` → cancel with no refund (used for cash/manual).
- `refundAmount > payment.Amount - payment.RefundedAmount` → `400`.
- Payment has no numeric `PaymobTransactionId` and `refundAmount > 0` → `400` with a message telling the admin to refund offline and cancel with `0`.
- Payment has no numeric `PaymobTransactionId` and `refundAmount == 0` → cancel, add note `"Cancelled by admin; refund handled outside Paymob."`.
- Audit: `CancellationSource = Admin`, `CancelledByUserId = currentUser`, `CancellationReason = body.Reason`.
- Everything else (claim, Paymob call, retry, review) is the same shared engine.

**Guard the existing admin refund action**

`RefundPaymentCommandHandler` must reject when:

```csharp
payment.RefundStatus is PaymentRefundStatus.Pending
                       or PaymentRefundStatus.Processing
                       or PaymentRefundStatus.NeedsReview
|| payment.RefundedAmount > 0
```

so the generic payments screen cannot double-refund a subscription.

## 9. Retry & Ambiguous Outcomes

`RefundRetryJob.RetryRefundAsync` gains two clearly separated branches:

```csharp
if (payment == null) return;

// Subscription pro-rated refund (new)
if (payment.RefundStatus == PaymentRefundStatus.Pending)
{
    // amount = payment.RefundAmount, never payment.Amount
    // Pending -> Processing claim, then Paymob
    // success -> CompleteRefund; failure -> FailRefund + reschedule
    // attempts >= MaxAttempts -> RefundStatus = Failed + admin alert (no more retries)
    return;
}

// Legacy appointment refund (unchanged behavior)
if (payment.Status != PaymentStatus.Paid) return;
var amountToRefund = payment.Amount;
```

Hard rules:

- Never process `Processing` or `NeedsReview` automatically.
- Never use `payment.Amount` for a subscription refund.
- `Processing` older than 15 minutes with no completion is flipped to `NeedsReview` by the retry job and reported to admins (the job can check `RefundRequestedAt`).

## 10. API Contracts

### 10.1 Quote — `GET /api/v1/subscriptions/my/cancellation-quote`

```json
{
  "subscriptionId": "3f2a...",
  "eligible": true,
  "reason": null,
  "planName": "Basic",
  "startDate": "2026-10-01T00:00:00",
  "endDate": "2026-10-31T00:00:00",
  "totalDays": 30,
  "usedDays": 2,
  "amountPaid": 200.00,
  "chargeAmount": 13.33,
  "refundAmount": 186.67,
  "currency": "EGP",
  "refundStatus": "None"
}
```

- When not eligible, return `200` with `eligible: false` and a localized `reason` (cash subscription, expired, no payment, etc.). The UI needs the reason to render, not an exception.
- No side effects. Safe to poll.

### 10.2 Cancel — `POST /api/v1/subscriptions/my/cancel`

```json
{
  "subscriptionId": "3f2a...",
  "cancelledAt": "2026-10-03T10:15:00",
  "totalDays": 30,
  "usedDays": 2,
  "chargedAmount": 13.33,
  "refundAmount": 186.67,
  "currency": "EGP",
  "refundStatus": "Completed",
  "refundId": "987654321"
}
```

- `refundStatus` is `None`, `Completed`, `Pending`, `Failed`, or `NeedsReview`.
- The endpoint returns `200` even when the refund is `Pending`/`Failed`/`NeedsReview` — the cancellation succeeded; only the money is still in progress.
- Response shape changes from the old `bool` to this object. Frontend must be updated.

### 10.3 Admin revoke — existing route, new optional body

See section 8. Response uses the same `CancelSubscriptionResponseDto`.

## 11. Reporting Impact

Adding `PartiallyRefunded` means these queries must change, or refunded subscriptions vanish from revenue instead of reducing it:

| Handler | Change |
|---|---|
| `GetAdminPaymentStatsQueryHandler.cs` | include `Paid` **and** `PartiallyRefunded`; revenue sums `Amount - RefundedAmount`; `RefundedCount` counts both; `SuccessCount` counts both (or keep `Paid` only — document the choice) |
| `GetRevenueTrendQueryHandler.cs` | include `PartiallyRefunded`; project `Net = Amount - RefundedAmount`; group by `Net` |
| `GetClinicsPaymentsSummaryQueryHandler.cs` | include `PartiallyRefunded`; projection uses `Amount - RefundedAmount` for `Gross` |
| `GetAdminPaymentsQueryHandler.cs` / `GetAdminPaymentDetailQueryHandler.cs` | map `RefundedAmount` and `RefundStatus` into `AdminPaymentDto` / `AdminPaymentDetailDto` |
| `AdminPaymentDto.cs` / `AdminPaymentDetailDto.cs` | add `decimal RefundedAmount`, `PaymentRefundStatus RefundStatus` |
| `SubscriptionDto.cs` | add `CancelledAt`, `RefundAmount`, `RefundStatus` (from linked payment) for history screens |

Appointment and ads revenue logic is **not** touched: this feature only creates partial refunds for `PaymentType.Subscription`.

## 12. File Structure Map

**Create**

- `ClinicHub.Domain/Enums/PaymentRefundStatus.cs`
- `ClinicHub.Domain/Enums/SubscriptionCancellationSource.cs`
- `ClinicHub.Application/Features/Subscriptions/Services/SubscriptionRefundCalculator.cs`
- `ClinicHub.Application/Features/Subscriptions/DTOs/SubscriptionCancellationQuoteDto.cs`
- `ClinicHub.Application/Features/Subscriptions/DTOs/CancelSubscriptionResponseDto.cs`
- `ClinicHub.Application/Features/Subscriptions/Queries/GetMyCancellationQuote/GetMyCancellationQuoteQuery.cs`
- `ClinicHub.Application/Features/Subscriptions/Queries/GetMyCancellationQuote/GetMyCancellationQuoteQueryHandler.cs`
- `ClinicHub.Application/Common/Interfaces/ISubscriptionCancellationService.cs`
- `ClinicHub.Application/Common/Services/SubscriptionCancellationService.cs`
- `ClinicHub.Persistence/Migrations/SqlServer/<timestamp>_AddSubscriptionProRatedRefunds.cs` (generated)

**Modify**

- `ClinicHub.Domain/Enums/PaymentStatus.cs`
- `ClinicHub.Domain/Enums/NotificationType.cs`
- `ClinicHub.Domain/Entities/Payment.cs`
- `ClinicHub.Domain/Entities/Subscription.cs`
- `ClinicHub.Persistence/Configuration/PaymentConfiguration.cs`
- `ClinicHub.Persistence/Configuration/SubscriptionConfiguration.cs`
- `ClinicHub.Application/DependencyInjection.cs`
- `ClinicHub.Application/Features/Subscriptions/Commands/CancelMySubscription/CancelMySubscriptionCommandHandler.cs`
- `ClinicHub.Application/Features/Subscriptions/Commands/CancelMySubscription/CancelMySubscriptionCommand.cs`
- `ClinicHub.Application/Features/Subscriptions/Commands/RevokeSubscription/RevokeSubscriptionCommand.cs`
- `ClinicHub.Application/Features/Subscriptions/Commands/RevokeSubscription/RevokeSubscriptionCommandHandler.cs`
- `ClinicHub.Application/Features/Subscriptions/Commands/RevokeSubscription/RevokeSubscriptionCommandValidator.cs`
- `ClinicHub.Application/Features/AdminPayments/Commands/RefundPayment/RefundPaymentCommandHandler.cs`
- `ClinicHub.Application/Features/AdminPayments/DTOs/AdminPaymentDto.cs`
- `ClinicHub.Application/Features/AdminPayments/DTOs/AdminPaymentDetailDto.cs`
- `ClinicHub.Application/Features/AdminPayments/Queries/GetAdminPayments/GetAdminPaymentsQueryHandler.cs`
- `ClinicHub.Application/Features/AdminPayments/Queries/GetAdminPaymentDetail/GetAdminPaymentDetailQueryHandler.cs`
- `ClinicHub.Application/Features/AdminPayments/Queries/GetAdminPaymentStats/GetAdminPaymentStatsQueryHandler.cs`
- `ClinicHub.Application/Features/Admin/Queries/GetRevenueTrend/GetRevenueTrendQueryHandler.cs`
- `ClinicHub.Application/Features/AdminPayments/Queries/GetClinicsPaymentsSummary/GetClinicsPaymentsSummaryQueryHandler.cs`
- `ClinicHub.Application/Features/Subscriptions/DTOs/SubscriptionDto.cs`
- `ClinicHub.Application/Features/Subscriptions/SubscriptionProfile.cs`
- `ClinicHub.Application/Localization/LocalizationKeys.cs`
- `ClinicHub.Application/Localization/Resources/messages.en.json`
- `ClinicHub.Application/Localization/Resources/messages.ar.json`
- `ClinicHub.Infrastructure/Services/BackgroundJobs/RefundRetryJob.cs`
- `ClinicHub.Infrastructure/Services/NotificationBuilderService.cs`
- `ClinicHub.API/Routes/ApiRoutes.cs`
- `ClinicHub.API/Controllers/Version1/SubscriptionsController.cs`
- `ClinicHub.API/Controllers/Version1/SuperAdminController.cs`

---

## 13. Tasks

### Task 1: Domain enums and entities

**Files:** create the two enums; modify `PaymentStatus.cs`, `Payment.cs`, `Subscription.cs`, `NotificationType.cs`.

- [ ] **Step 1:** Add `PartiallyRefunded = 5`, `RefundPending = 6`, `RefundFailed = 7` to `PaymentStatus` (append only).
- [ ] **Step 2:** Create `PaymentRefundStatus` with `None, Pending, Processing, Completed, Failed, NeedsReview`.
- [ ] **Step 3:** Create `SubscriptionCancellationSource` with `ClinicOwner, Admin, System`.
- [ ] **Step 4:** Add the seven refund fields and the five methods from section 6.4 to `Payment`. Add `using` for the new enum namespace.
- [ ] **Step 5:** Add the five cancellation/refund fields from section 6.5 to `Subscription`.
- [ ] **Step 6:** Append `SubscriptionCancelled = 22` to `NotificationType` (21 is `AppointmentNoShow`).

**Verify:** `dotnet build ClinicHub.slnx` — the solution must compile before moving on.

---

### Task 2: Persistence configuration and migration

**Files:** `PaymentConfiguration.cs`, `SubscriptionConfiguration.cs`.

- [ ] **Step 1:** Configure decimal precision and string lengths exactly as in section 6.5.
- [ ] **Step 2:** Add `builder.HasIndex(p => p.RefundStatus);` on `Payment`.
- [ ] **Step 3:** Generate the migration:

```powershell
dotnet ef migrations add AddSubscriptionProRatedRefunds --project ClinicHub.Persistence --startup-project ClinicHub.API
```

- [ ] **Step 4:** Inspect the generated `Up()`: it must contain **only** `AddColumn` operations + the index. No drops, no table rebuilds.
- [ ] **Step 5:** Apply locally:

```powershell
dotnet ef database update --project ClinicHub.Persistence --startup-project ClinicHub.API
```

**Verify:** `dotnet ef migrations list` shows the new migration as applied. `SELECT TOP 1 RefundStatus, RefundAmount, RefundedAmount FROM Payments;` succeeds. `RefundAmount` and `RefundedAmount` are `0` for existing rows.

---

### Task 3: Refund calculator and quote endpoint

**Files:** create the calculator, quote DTO, query, handler; modify `ApiRoutes.cs`, `SubscriptionsController.cs`.

- [ ] **Step 1:** Create `SubscriptionRefundCalculator.Calculate` exactly as in section 5.
- [ ] **Step 2:** Create `SubscriptionCancellationQuoteDto` with the fields from section 10.1 (`RefundStatus` as `bool`/string is fine; keep it serializable).
- [ ] **Step 3:** Create `GetMyCancellationQuoteQuery` / handler:
  - resolve `CurrentClinicId`; load the active subscription + payment (`Include(s => s.Payment)`).
  - if no active subscription → return `Eligible = false` with the localized reason.
  - if payment missing / not online / status not refundable → `Eligible = false`.
  - otherwise calculate and return the quote.
  - Do **not** mutate anything.
- [ ] **Step 4:** Add `CancellationQuote = BaseRoute + "/my/cancellation-quote"` to `ApiRoutes.Subscriptions`.
- [ ] **Step 5:** Add the `[HttpGet]` action to `SubscriptionsController` with `[RoleAuthorize(nameof(UserType.ClinicOwner))]`.

**Verify (manual, with a seeded test DB):**
- 200 EGP monthly, cancel 2 days later → `13.33 / 186.67`.
- Same-day → `6.67 / 193.33`.
- 31-day month → `12.90 / 187.10`.
- Cash-paid subscription → `eligible: false` + reason.
- Expired subscription → `eligible: false` + reason.

---

### Task 4: Shared cancellation engine and owner endpoint

**Files:** create `ISubscriptionCancellationService` + `SubscriptionCancellationService`; modify `DependencyInjection.cs`, the cancel command + handler, `SubscriptionsController.cs`.

- [ ] **Step 1:** Define the interface:

```csharp
public interface ISubscriptionCancellationService
{
    Task<CancelSubscriptionResponseDto> CancelAsync(
        Guid subscriptionId,
        Guid? clinicIdScope,
        SubscriptionCancellationSource source,
        Guid? cancelledByUserId,
        decimal? refundAmountOverride,
        string? reason,
        bool requireOnlinePayment,
        CancellationToken cancellationToken);

    Task RefundAsync(Guid paymentId, CancellationToken cancellationToken);
}
```

- [ ] **Step 2:** Implement `CancelAsync`:
  - load subscription (verify `clinicIdScope` when provided) and its payment
  - validate state, expiry, online requirement, refundability
  - compute the quote
  - apply the override rules from section 8
  - `BeginTransactionAsync()` → mutate subscription + `Payment.RequestRefund` → `SaveChangesAsync()` → `CommitAsync()`
  - if refund > 0, call `RefundAsync(payment.Id)` **after** the commit
  - map to `CancelSubscriptionResponseDto`
- [ ] **Step 3:** Implement `RefundAsync` exactly as section 7 step 8 (gate → re-read → `ClaimRefund` → Paymob → `CompleteRefund` / `FailRefund` / `RequireRefundReview`).
- [ ] **Step 4:** Register the service in `ClinicHub.Application/DependencyInjection.cs` (scoped).
- [ ] **Step 5:** Replace the body of `CancelMySubscriptionCommandHandler` with a thin call to the engine. Add `string? Reason` to `CancelMySubscriptionCommand` (optional).
- [ ] **Step 6:** Change the controller action's return type/serialization to `CancelSubscriptionResponseDto`.
- [ ] **Step 7:** Send the owner FCM notification and superadmin notifications after the operation (Task 8 covers the template; call sites belong here).

**Verify:**
- Cash subscription → `400`.
- Online subscription → `200`, `subscriptions.status = Cancelled`, `payments.RefundStatus = Completed/Pending`, `payments.RefundAmount = 186.67`.
- Two parallel `POST /my/cancel` → one `200`, the other `400`/`409`.
- Repeated cancel after success → `404` (no active subscription).

---

### Task 5: Admin cancellation and refund guards

**Files:** `RevokeSubscriptionCommand.cs`, its handler and validator, `SuperAdminController.cs`, `RefundPaymentCommandHandler.cs`.

- [ ] **Step 1:** Add `decimal? RefundAmount` and `string? Reason` to `RevokeSubscriptionCommand`.
- [ ] **Step 2:** Add validator rules: `RefundAmount >= 0` when present; `Reason` max 500.
- [ ] **Step 3:** Rewrite the handler to call the engine with `source: Admin`, `clinicIdScope: null`, `requireOnlinePayment: false`.
- [ ] **Step 4:** Update `SuperAdminController.RevokeSubscription` to accept `[FromBody] RevokeSubscriptionCommand` with the id on the route:

```csharp
public async Task<IActionResult> RevokeSubscription(Guid id, [FromBody] RevokeSubscriptionCommand? command, CancellationToken ct)
{
    var request = command ?? new RevokeSubscriptionCommand();
    request.SubscriptionId = id;
    var result = await _mediator.Send(request, ct);
    return Ok(result);
}
```

- [ ] **Step 5:** Add the double-refund guards to `RefundPaymentCommandHandler` from section 8.

**Verify:**
- Pro-rated default → refund equals quote.
- Override 100.00 → Paymob receives 100.00.
- Override > paid → `400`.
- Cash subscription + override 0 → cancelled, no Paymob call, note recorded.
- Cash subscription + override > 0 → `400`.
- Admin payments screen refund on a partially refunded subscription → `400`.

---

### Task 6: Retry job and ambiguous-outcome handling

**Files:** `RefundRetryJob.cs` (and, if needed, `BackgroundJobScheduler.cs` — `ScheduleRefundRetryAsync` already exists).

- [ ] **Step 1:** Add the subscription branch (`RefundStatus == Pending`) before the legacy `Status == Paid` branch.
- [ ] **Step 2:** Use `payment.RefundAmount`, never `payment.Amount`.
- [ ] **Step 3:** Claim via `ClaimRefund()` inside `PaymentRefundGate`; bail out if the claim fails.
- [ ] **Step 4:** On success call `CompleteRefund`, on definitive failure call `FailRefund` + reschedule, and after `MaxAttempts` set `Failed` and notify admins.
- [ ] **Step 5:** Add a stale-`Processing` sweep: if `RefundStatus == Processing` and `RefundRequestedAt < now - 15 min`, set `NeedsReview` and notify admins. Never auto-retry `NeedsReview`.

**Verify:**
- Force `PaymobTransactionId = "invalid"` → job leaves refund `Pending`/`Failed`, cancellation stays `Cancelled`.
- Kill the API after the `Processing` save but before the Paymob response (simulate by setting `RefundStatus = Processing` and an old `RefundRequestedAt` in SQL) → job flips it to `NeedsReview`, no Paymob call.
- Legitimate appointment cancellation still refunds `payment.Amount` (regression check).

---

### Task 7: Reporting net of refunds

**Files:** the five handlers/DTOs listed in section 11.

- [ ] **Step 1:** `GetAdminPaymentStatsQueryHandler`: include `PartiallyRefunded`; sum `Amount - RefundedAmount`; count both refunded statuses.
- [ ] **Step 2:** `GetRevenueTrendQueryHandler`: same inclusion; group on the net amount.
- [ ] **Step 3:** `GetClinicsPaymentsSummaryQueryHandler`: same inclusion; `Gross` is net.
- [ ] **Step 4:** Add `RefundedAmount` + `RefundStatus` to both admin payment DTOs and map them in the two query handlers.
- [ ] **Step 5:** Add `CancelledAt`, `RefundAmount`, `RefundStatus` to `SubscriptionDto` and map them (`SubscriptionProfile.cs`); use `Payment` include where the DTO is produced.

**Verify (SQL arithmetic):**
- After a 186.67 refund on a 200 payment, admin subscription revenue shows `13.33`, not `200` and not `0`.
- `RefundedCount` increases by 1 for a partial refund.
- Appointment revenue is unchanged.

---

### Task 8: Notifications and localization

**Files:** `NotificationBuilderService.cs`, `LocalizationKeys.cs`, `messages.en.json`, `messages.ar.json`.

- [ ] **Step 1:** Add a `case NotificationType.SubscriptionCancelled:` template in Arabic:
  - title: `تم إلغاء الاشتراك`
  - body includes `clinicName`, refund amount, and whether the refund is pending or completed
  - link: notifications/billing deep link, **not** appointment details
- [ ] **Step 2:** Add localization keys (English + Arabic):

```text
Subscriptions.OnlyOnlineCanCancel
Subscriptions.ExpiredCannotCancel
Subscriptions.NoPaymentLinked
Subscriptions.AlreadyCancelled
Subscriptions.RefundExceedsPaid
Subscriptions.CancelledWithRefund
Subscriptions.CancelledNoRefund
Subscriptions.RefundPending
Subscriptions.RefundNeedsReview
Payments.RefundInProgress
Payments.RefundNeedsReview
```

- [ ] **Step 3:** Notify:
  - owner on cancellation (amount + status)
  - superadmins on owner cancellation (visibility)
  - superadmins on refund failure / needs review

**Verify:** switch `Accept-Language: ar` and `en`; messages render correctly; a new unknown type still falls back to the default template (no exception).

---

### Task 9: End-to-end verification

No test project exists. Run this checklist against a local SQL Server with Paymob **test** credentials.

- [ ] `dotnet build ClinicHub.slnx` → 0 errors.
- [ ] Migration applied; old payment rows have `RefundedAmount = 0`.
- [ ] Quote math matches all five rows in section 5.
- [ ] Owner cash cancel → `400`.
- [ ] Owner online cancel → subscription `Cancelled`, payment `PartiallyRefunded`/`Refunded`, correct `RefundedAmount`.
- [ ] Double-click → exactly one refund.
- [ ] Admin cancel with override → exact override refunded.
- [ ] Admin cancel with override 0 → no Paymob call.
- [ ] Ambiguous outcome → `NeedsReview`, no retry scheduled.
- [ ] Revenue reports net the refund.
- [ ] Arabic + English responses.
- [ ] `GET /subscriptions/my` no longer returns the cancelled subscription as active (`RequireSubscriptionAttribute` denies access).
- [ ] Hangfire dashboard shows no failed refund job after a happy-path run.

## 14. Risks & Reversal

- **Refunds are irreversible.** Validate partial refunds on Paymob test credentials with a small amount before enabling in production. Confirm the merchant account allows partial refunds on the same transaction.
- **Dual-write window** (Paymob succeeded, DB save failed) is handled by `NeedsReview` + manual reconciliation. Never "just retry" an ambiguous refund.
- **API contract change:** `POST /subscriptions/my/cancel` now returns an object. Update the frontend in the same release.
- **Migration is additive.** To roll back: revert the code, then `dotnet ef migrations remove` if not applied, or apply the generated `Down()` (drops refund/cancellation columns and the index only).
- **Reporting numbers change** by design: partial refunds now reduce revenue instead of deleting the whole payment. Announce this to whoever reads the dashboards.
- **Old bug fixed in passing:** `RevokeSubscriptionCommandHandler` stores the Paymob refund id in `RefundReason`; new code uses `RefundId` and keeps `RefundReason` for the human reason.

## 15. Open Questions / Defaults

These are implemented with the stated defaults unless changed before coding:

1. **Day basis:** real calendar days. Switching to a fixed 30-day month is a one-line change in the calculator.
2. **Minimum charge:** 1 day for same-day cancellation.
3. **Admin override:** allowed, capped at `Amount - RefundedAmount`.
4. **Cash/manual subscription:** admin cancels with refund 0 and a note; owner cannot self-cancel.
5. **`NeedsReview` resolution:** handled by an admin manually (SQL + Paymob dashboard) in v1. A dedicated resolve endpoint can be added later; it is not part of this plan.
6. **Refund timing message:** "5–7 business days, depending on the bank/wallet".
7. **`SuccessCount` in admin stats:** counts `Paid + PartiallyRefunded` (the payment did succeed). If finance wants gross-success only, change to `Paid` and document it.
