# Clinic Payouts (Paymob Send) — Implementation Plan

Date: 2026-10-25
Status: Proposed
Owner: Backend

## 1. Goal

Automatically transfer each clinic's net appointment revenue (consultation fees minus
platform fee, refunds and adjustments) to the clinic's bank account using the Paymob
Payout (Send) disbursement API, with an auditable statement per clinic per cycle.

Money moves only after we can prove the balance: a ledger is the source of truth, not a
re-computation over `Payments` at payout time.

## 2. Paymob Payout (Send) — what we integrate with

Payout is a separate Paymob product/account from Accept (collections). It is enabled by a
Paymob account manager after a contract + KYC.

### 2.1 Environments

| Env | Base URL |
|---|---|
| Staging (Egypt) | `https://stagingpayouts.paymobsolutions.com` |
| Production (Egypt) | `https://payouts.paymobsolutions.com` |

### 2.2 Authentication

`POST {BaseUrl}/api/secure/o/token/` — `application/x-www-form-urlencoded`

```
client_id, client_secret, username, password, grant_type=password
```

Response: `access_token`, `refresh_token`, `expires_in`, `token_type`.
The access token is sent as `Authorization: Bearer {access_token}` on every call and is
refreshed before expiry (docs: valid ~60 minutes). Cache it in memory
(`IMemoryCache`) with a 5-minute safety margin; refresh on 401 once and retry.

### 2.3 Disbursement

`POST {BaseUrl}/api/secure/disburse/` — Bearer token, `application/json`

Bank payout (the one we need):

```json
{
  "issuer": "bank_card",
  "amount": "120.00",
  "full_name": "Clinic Legal Name",
  "bank_card_number": "<account/card number>",
  "bank_code": "CIB",
  "bank_transaction_type": "cash_transfer"
}
```

- `bank_code` is a closed enum (CIB, NBE, MISR, QNB, AUB, HSBC, AAIB, …) — validate on
  save, never accept free text.
- `bank_transaction_type`: `cash_transfer` | `salary` | `credit_card` | `prepaid_card`
  (default `cash_transfer`).
- `amount` is a **string** with 2 decimals.
- Response: `transaction_id` (uuid) + `status`.

Wallet payout (fallback / future): `issuer` = `vodafone|etisalat|orange|bank_wallet` with
`msisdn`. Aman also supported (`issuer: "aman"`), not required now.

### 2.4 Inquiry (reconciliation)

`POST {BaseUrl}/api/secure/transaction/inquire/`

```json
{ "transactions_ids_list": ["<uuid>", "..."] }
```

Response `results[]` with `disbursement_status` ∈ `pending | successful | failed`.

### 2.5 Budget (float balance)

`GET {BaseUrl}/api/secure/budget/inquire/` → `current_budget`, `status_code`,
`status_description` (`"Your current budget is 888.25 LE"`).

Disbursements are debited from this prepaid budget. Every batch must be budget-checked
before execution.

### 2.6 Callback

Registered once in the Paymob payout dashboard (sidebar → callback URL). Payload contains
`transaction_id`, `issuer`, `amount`, `status_code`, `disbursement_status`. The docs do not
document an HMAC, so the callback is treated as a **hint**: correctness comes from
re-inquiring the transaction before settling money in our books.

### 2.7 Known constraints to confirm with Paymob

1. Minimum per transaction: documented 112 EGP for anonymous cash-in; confirm the bank
   transfer minimum and whether it is per transaction or per batch.
2. Fees per bank transfer and who bears them (platform or clinic) — affects the net amount
   we promise the clinic.
3. Whether a **client-supplied reference / idempotency key** exists (the public schema has
   none). If not, our retry strategy must always inquire first (see §7.3).
4. Callback signature/HMAC and IP allowlist.
5. How the payout budget is funded: automatic sweep of Accept collections vs manual
   top-up; and the settlement delay between collection and availability.
6. Production URL + credentials and any daily/monthly limits.

### 2.8 Credentials to request (and where they are sent)

Paymob sends staging credentials by email to the address given during onboarding. Use a
company mailbox owned by finance/ops (for example `finance@<company-domain>`), not a
personal inbox, and add a shared alias so it survives staff changes. Required values:

- `client_id`, `client_secret`, `username`, `password` (staging + production)
- Portal login for `stagingpayouts.paymobsolutions.com`
- Confirmation that the Payout product is enabled on both accounts
- The test data they provide for bank transfers in staging (test bank accounts/codes)

## 3. Domain model

### 3.1 New entities

`ClinicPayoutAccount` (1:1 with `Clinic`)

| Field | Notes |
|---|---|
| ClinicId | unique index |
| HolderName | bank account holder, must match KYC name |
| BankCode | validated against Paymob enum |
| AccountNumberEncrypted | encrypted at rest (Data Protection), never returned raw |
| AccountLast4 | for masking in UI/statements |
| BankTransactionType | default `cash_transfer` |
| Status | `PendingVerification`, `Verified`, `Rejected`, `Disabled` |
| VerifiedByUserId / VerifiedAt / RejectionReason | admin verification trail |

`ClinicLedgerEntry` — the balance source of truth

| Field | Notes |
|---|---|
| ClinicId | index |
| Type | `Earning`, `RefundDeduction`, `Payout`, `Adjustment`, `Reversal` |
| Amount | signed decimal(18,2) |
| Currency | EGP |
| Status | `Available`, `Reserved`, `Settled`, `Cancelled` |
| PaymentId / PayoutItemId | idempotency anchors |
| AvailableAt | hold period end (refund window) |
| Notes | free text |

Unique indexes: `(PaymentId, Type)` for earnings, `(PayoutItemId)` for payout rows. This is
what makes "never pay twice" a database guarantee, not application logic.

`PayoutBatch` — one settlement run (a "كشف")

| Field | Notes |
|---|---|
| Code | human reference, e.g. `PO-2026-10-25-01` |
| Status | `Draft`, `AwaitingApproval`, `Approved`, `Processing`, `Completed`, `PartiallyFailed`, `Failed`, `Cancelled` |
| PeriodFrom / PeriodTo | earnings window |
| TotalAmount / ItemsCount | snapshot |
| BudgetBefore / BudgetAfter | from Paymob budget inquiry |
| CreatedBy / ApprovedBy / ApprovedAt | two-man rule |
| ExecutedAt / FailureNotes | |

`PayoutItem` — one clinic inside a batch

| Field | Notes |
|---|---|
| BatchId / ClinicId / PayoutAccountId | |
| Amount | equals the reserved ledger sum |
| Status | `Pending`, `Processing`, `Success`, `Failed`, `Cancelled` |
| PaymobTransactionId | unique nullable — idempotency |
| PaymobStatusCode / DisbursementStatusRaw | raw Paymob values |
| Attempts / LastAttemptAt / LastError | retry trail |
| SentAt / SettledAt | |

`PayoutWebhookEvent` — raw callback audit + replay protection

`PaymobTransactionId` (unique), `RawPayload`, `StatusCode`, `DisbursementStatus`,
`ReceivedAt`, `ProcessedAt`, `Error`.

### 3.2 Changes to existing `Payment`

Add snapshotted split columns:

- `PlatformFeeAmount` decimal(18,2)
- `ClinicNetAmount` decimal(18,2)

Reason: revenue today is split with the **current** platform percentage
(`AppointmentRevenueSplitter.Split(p.Amount, percent)` in dashboards/notifications), so
changing the fee percent retroactively changes historical net revenue. Payouts must use the
value that was true when the payment was collected.

- Populate in `Payment.MarkAsPaid` / webhook with the percent in effect at that moment.
- Backfill existing Paid appointment payments with the current percent (matches what the
  dashboards show today) in the same migration.
- Update dashboard, payments list, revenue trend, and notification builders to read the
  snapshot instead of recomputing.

### 3.3 Ledger derivation rules

- On payment paid: `Earning = ClinicNetAmount`, `AvailableAt = PaidAt + RefundHoldDays`
  (default 7 days, configurable) to cover the refund window.
- On refund of a paid appointment (after payout): `RefundDeduction = -ClinicNetAmount`,
  immediately `Available` (negative balance rolls into the next cycle).
- On batch create: each clinic's available entries are marked `Reserved`.
- On item success: `Settled`. On item failure/cancel: back to `Available`.
- Available balance = `Σ Available entries` (signed). Never computed from raw payments.

## 4. Application layer (CQRS)

### 4.1 Clinic endpoints

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/v1/clinic/payouts/summary` | available, reserved, paid-this-year, next settlement date, account status |
| GET | `/api/v1/clinic/payouts` | batch history (paged) |
| GET | `/api/v1/clinic/payouts/{id}` | batch statement (items + ledger lines) |
| GET | `/api/v1/clinic/payout-account` | masked account + status |
| PUT | `/api/v1/clinic/payout-account` | save/update; resets status to `PendingVerification` |

### 4.2 Admin endpoints (SuperAdmin)

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/v1/admin/payouts` | batches list |
| GET | `/api/v1/admin/payouts/{id}` | batch detail + failures |
| POST | `/api/v1/admin/payouts/preview` | compute eligible clinics and totals (no writes) |
| POST | `/api/v1/admin/payouts` | create Draft + reserve ledger |
| POST | `/api/v1/admin/payouts/{id}/approve` | approve (approver ≠ creator) |
| POST | `/api/v1/admin/payouts/{id}/execute` | enqueue execution job |
| POST | `/api/v1/admin/payouts/{id}/items/{itemId}/retry` | retry one failed item |
| POST | `/api/v1/admin/payouts/{id}/cancel` | cancel draft/approved, release reserves |
| GET | `/api/v1/admin/payouts/budget` | live Paymob budget |

### 4.3 Public webhook

`POST /api/v1/payouts/webhook` — unauthenticated but:
- optionally restricted by IP allowlist,
- requires a secret path segment or header checked against configuration,
- never trusts payload for money movement — always re-inquires by `transaction_id`.

Excluded from rate limiting (same treatment as `/payments/webhook`).

### 4.4 Interfaces

`IPayoutService` (Application layer):

```
Task<string> GetAccessTokenAsync(CancellationToken ct);
Task<decimal> GetBudgetAsync(CancellationToken ct);
Task<PayoutDisbursementResult> DisburseBankAsync(PayoutRequest request, CancellationToken ct);
Task<IReadOnlyList<PayoutTransactionStatus>> InquireAsync(IReadOnlyCollection<string> transactionIds, CancellationToken ct);
```

`PaymobPayoutService` (Infrastructure) implements it with a dedicated `HttpClient`
(separate base URL and credentials from `PaymobService`).

## 5. Settlement cycle

1. **Accrual** — webhook marks a payment Paid → snapshot split → append `Earning` ledger row.
2. **Settlement job** (`PayoutSettlementJob`, weekly, e.g. Sunday 02:00 local):
   - collect clinics with `Available` balance ≥ `MinimumPayoutAmount` and
     `Verified` payout account,
   - create `Draft` batch + `PayoutItem` per clinic + reserve ledger rows,
   - notify admins for review.
3. **Approval** — SuperAdmin approves (creator ≠ approver when
   `RequireTwoManApproval=true`).
4. **Execution job** (`PayoutExecutionJob`):
   - `GetBudgetAsync` once; block the batch if `budget < total + buffer`,
   - for each item sequentially (1 req/s): disburse → persist `transaction_id` →
     `Processing`,
   - network/5xx errors: retry up to 3× with backoff after an inquiry (see §7.3),
   - business rejections: `Failed`, ledger released, item visible for manual retry.
5. **Confirmation** — callback and/or hourly `PayoutStatusReconciliationJob`:
   - `successful` → item `Success`, ledger `Settled`, clinic notification + email advice,
   - `failed` → item `Failed`, ledger `Available`, alert admins, optional auto-retry next
     cycle (max 2 attempts).
6. **Close** — batch `Completed`/`PartiallyFailed`; store budget before/after; write audit.

## 6. Background jobs (Hangfire — existing infrastructure)

| Job | Schedule | Purpose |
|---|---|---|
| `PayoutSettlementJob` | `Cron.Weekly(DayOfWeek.Sunday, 2)` | create draft batches |
| `PayoutExecutionJob` | on demand (`BackgroundJob.Enqueue`) | disburse approved batch |
| `PayoutStatusReconciliationJob` | `Cron.Hourly` | inquire `Processing` items; settle/release |
| `PayoutBudgetAlertJob` | `Cron.Daily` | warn admins when budget < upcoming liability |

Register in `Program.cs` next to the existing recurring jobs; register services in
`Infrastructure/DependencyInjection.cs`.

## 7. Safety and correctness

### 7.1 Double-payment prevention

- Ledger rows are reserved by `PayoutItemId` (unique) — a clinic's earnings cannot be
  included in two batches.
- `PayoutItem.PaymobTransactionId` is unique.
- Execution is idempotent: an item already `Processing/Success` is skipped.

### 7.2 Two-man approval

`PayoutBatch.ApprovedBy` must differ from `CreatedBy` when enabled. Configured by
`PayoutSettings.RequireTwoManApproval`.

### 7.3 Ambiguous disbursement results

If a disburse call times out, we do not know whether Paymob accepted it. Before any retry:
inquiry by the stored `transaction_id` (or by reference if Paymob adds one). Only when the
transaction is confirmed absent is the item re-sent.

### 7.4 Money and rounding

- Paymob amounts are strings with 2 decimals; format with `InvariantCulture`.
- Ledger uses decimal(18,2); no floats anywhere.
- Platform fee is retained; only `ClinicNetAmount` is paid out. Transfer fees are either
  absorbed by the platform or deducted from net — decide in §2.7 item 2 and apply
  consistently in the statement.

### 7.5 Security

- Bank account numbers encrypted at rest (`IDataProtectionProvider`), masked everywhere
  (`**** 1234`), never logged.
- Clinic endpoints: `ClinicAdmin` only, scoped by `CurrentClinicId`.
- Admin endpoints: `SuperAdmin` via `RoleAuthorize`.
- Every account change, batch approval and execution writes an `AuditLog`.

### 7.6 Refunds and adjustments

Refunds after payout create negative ledger entries. If a clinic's balance goes negative,
the next settlement nets it; never claw back from an already-sent transfer automatically.
Document this in the clinic-facing statement.

## 8. Configuration

### 8.1 New options class `PayoutSettings`

```json
"PayoutSettings": {
  "BaseUrl": "https://stagingpayouts.paymobsolutions.com",
  "ClientId": "",
  "ClientSecret": "",
  "Username": "",
  "Password": "",
  "CallbackUrl": "https://<api-host>/api/v1/payouts/webhook",
  "CallbackSecret": "<random-32-bytes>",
  "DefaultBankTransactionType": "cash_transfer",
  "MinimumPayoutAmount": 120,
  "MinimumTransactionAmount": 112,
  "RefundHoldDays": 7,
  "SettlementDay": "Sunday",
  "SettlementHour": 2,
  "RequireTwoManApproval": true,
  "AutoExecute": false,
  "MaxRetriesPerItem": 2
}
```

- Bind and validate at startup exactly like `PaymobSettings` (`Program.cs`), so a missing
  credential fails fast instead of at payout time.
- Production secrets via environment variables or the host's secret store:
  `PayoutSettings__ClientSecret`, `PayoutSettings__Password`, … Never commit them.
- `AutoExecute=false` for launch: settlement creates drafts, execution stays manual until
  the process is trusted.

### 8.2 Paymob dashboard configuration (staging, then production)

1. Account manager enables Payout (Send); contract + KYC completed.
2. Receive portal login and API credentials by email (company mailbox).
3. Register the callback URL (`https://<api-host>/api/v1/payouts/webhook`) in the portal.
4. Ask for the source IPs to allowlist; mirror them in the API firewall if provided.
5. Create a disbursement API user with the minimum role needed.
6. Fund the staging budget (top-up request) so end-to-end tests can run.
7. Confirm with Paymob the test bank accounts/codes usable in staging.

### 8.3 Local/staging reachability

Paymob must reach the callback URL over public HTTPS. For local testing expose the API via
a tunnel (ngrok/cloudflared) and register that URL temporarily; never register a tunnel URL
in production.

## 9. Data migration / backfill

1. EF migration: new tables + `Payment.PlatformFeeAmount` / `Payment.ClinicNetAmount` +
   indexes.
2. Backfill SQL inside the migration:
   - for every Paid appointment payment with null snapshot columns, compute
     `net = ROUND(Amount / (1 + currentPercent/100), 2)` and
     `fee = Amount - net`,
   - insert one `Earning` ledger row per payment (`Available`, `AvailableAt = PaidAt`),
   - guard with `WHERE NOT EXISTS` so re-running is safe.
3. Update all revenue readers to use the snapshot columns (dashboard, payments list,
   revenue trend, notifications, admin payments summary).

Note: the backfill uses today's platform percent for history; this matches what the
dashboards already display. Snapshot columns make all future changes exact.

## 10. Testing

No test project exists today. Minimum manual/staging checklist:

- Save a payout account → masking + encryption round-trip.
- Preview/creation with: clinic below minimum, clinic without verified account, clinic with
  negative balance, clinic with refund after payout.
- Budget guard: batch total > budget → execution blocked, nothing sent.
- Disburse happy path → callback → item Success → ledger Settled → clinic notification.
- Callback replay → no double settle.
- Timeout simulation → inquiry path → no double payment.
- Failed bank code / rejected transfer → ledger released, retry works.
- Concurrent execution trigger → Hangfire single run + DB uniqueness prevents duplicates.
- Reconciliation job picks up a missed callback.
- Verify statements/report totals equal the sum of ledger entries.

## 11. Rollout phases

| Phase | Scope | Est. |
|---|---|---|
| 0 | Paymob onboarding, contract/KYC, staging credentials, budget top-up | external |
| 1 | Schema, ledger, snapshot columns + backfill, clinic payout account CRUD, admin preview (no money movement) | 3–4 days |
| 2 | Paymob client, disbursement, callback, execution + reconciliation jobs | 4–6 days |
| 3 | Notifications/emails, statements, hardening, staging E2E | 2–3 days |
| 4 | Production launch with manual approval; enable weekly auto-settlement after 2–4 clean cycles | — |

## 12. Open decisions

1. Transfer fee bearer: platform or clinic (changes the net users see).
2. Settlement cadence: weekly vs bi-weekly vs monthly, and the cut-off time.
3. Refund hold period (`RefundHoldDays`) vs clinic expectations.
4. Whether clinics can change bank details without re-verification (recommended: no).
5. Whether partial payouts (below threshold) should roll over indefinitely.
