# ClinicHub — Appointment Payments & Ads — Complete API Workflow

> **Stack:** ASP.NET Core 10 • MediatR CQRS • EF Core + SQL Server + NetTopologySuite • Paymob Intention API UnifiedCheckout  
> **Date:** 2026-08-29 — **Branch:** `fix/push-main-block` (includes `142bc7f` mapper fix, `5167292` diagnostics, `fc26ce6`/`439291c` docs)  
> **Base URL:** `{host}/api/v{version}` → `v1` is `api/v1`. Docs at `/scalar/v1`, root `/` redirects to latest.  
> **Default culture:** Arabic (`Accept-Language: ar` / `en`). All user-facing errors via `IStringLocalizer<Messages>` (`ClinicHub.Application/Localization/Resources/messages.{en,ar}.json`).

This is the **single source of truth** — it replaces all previous fragmented docs (`appointment-payment-mobile-workflow.md`, `MOBILE_APPOINTMENT_ADS_INTEGRATION.md`, `paymob-integration-mapping.md`, `ADS_FRONTEND.md`, etc.) which were removed on 2026-08-29.

---

## Table of Contents

1. [Overview & What Changed](#1-overview--what-changed)
2. [Enums, Entities & Pricing](#2-enums-entities--pricing)
3. [Paymob Settings & Integration IDs](#3-paymob-settings--integration-ids)
4. [Routes Summary](#4-routes-summary)
5. [Appointment Payment Workflow](#5-appointment-payment-workflow)
6. [Request / Response Contracts](#6-request--response-contracts)
7. [Credit vs Wallet Branching](#7-credit-vs-wallet-branching)
8. [Paymob Service Internals](#8-paymob-service-internals)
9. [Webhook — Server Truth & Idempotency](#9-webhook--server-truth--idempotency)
10. [Clinic Accept Flows](#10-clinic-accept-flows)
11. [Ads Payments (Clinic Owner)](#11-ads-payments-clinic-owner)
12. [Subscription Parity](#12-subscription-parity)
13. [Mobile WebView, Polling & Deep Links](#13-mobile-webview-polling--deep-links)
14. [Error Mapping](#14-error-mapping)
15. [FCM Notifications](#15-fcm-notifications)
16. [Background Jobs](#16-background-jobs)
17. [cURL Examples](#17-curl-examples)
18. [Changed Files](#18-changed-files)
19. [Checklists](#19-checklists)

---

## 1. Overview & What Changed

**Goal:** Appointments can pay with **wallet** (`PaymobWallet` — Vodafone Cash / Orange / Etisalat via Paymob) **or credit card** (`PaymobCreditCard`) — same as subscriptions and ads. No internal ledger — `Wallet` = Paymob Wallet integration, not stored balance.

| Area | Before | After | Code |
|---|---|---|---|
| `POST /payments/initiate` | hardcoded `InitiateWalletPaymentAsync` (wallet only) | `paymentMethod` body field → wallet **or** card | `ClinicHub.Application/Features/Payment/Commands/InitiatePayment/InitiatePaymentCommand.cs:6` + `InitiatePaymentCommandHandler.cs:63` |
| `POST /payments` (booking `Reserved`) | hardcoded `InitiateCheckoutPaymentAsync` (card only) | `paymentMethod`+`returnUrl` → wallet or card, omitted → **card** (legacy) | `InitiateBookingPaymentCommand.cs:8` + `InitiateBookingPaymentCommandHandler.cs:65` |
| `PUT /appointments/{id}/accept` | hardcoded card | `?paymentMethod=wallet\|card&returnUrl=` query **or** JSON body, query wins, omitted → card | `ClinicHub.API/Controllers/Version1/AppointmentsController.cs:126` + `ClinicHub.Application/Common/Services/AppointmentAcceptanceService.cs:68` |
| `PUT /staff/appointments/{id}/approve` | same | same `?paymentMethod=&returnUrl=` | `ClinicHub.API/Controllers/Version1/StaffController.cs:53` |
| `PUT /doctors/appointments/{id}/accept` | same | same | `ClinicHub.API/Controllers/Version1/DoctorDashboardController.cs:66` |
| `PUT /doctors/appointments/{id}/status` with `status=6` | called `AcceptAsync` without method | forwards `paymentMethod`/`returnUrl` | `DoctorDashboardController.cs:107` |
| `PaymentMethodMapper` | only `paymob_card` with `_`; frontend sends `PaymobCreditCard` (no `_`) → fell to default wallet → **both flows showed wallet** | added `paymobcreditcard`, `paymob_creditcard`, `visa`, `mastercard` etc. | `ClinicHub.Application/Features/AdminPayments/PaymentMethodMapper.cs:15` (`142bc7f`) |
| `PaymobSettings` IDs | `IntegrationId=5690721` was wallet, `WalletIntegrationId=5690722` invalid `404` | corrected to `5671290` card (MIGS) + `5690721` wallet (UIG), both `201` | `ClinicHub.Infrastructure/Services/Paymob/PaymobService.cs:60,93` |
| `PaymobService` errors | `int.Parse` crash on placeholder `YOUR_...`, generic message | `ResolveIntegrationId` safe `TryParse`+`ILogger`+ surfaces Paymob `detail` | `ClinicHub.Infrastructure/Services/Paymob/PaymobService.cs:34,182` (`5167292`) |
| **Ads** | required active Premium/Enterprise subscription | only needs `Clinic.Status==Active` | `ClinicHub.Application/Features/Ads/AdsOrderProcessor.cs:123` |

No DB migration — `Payment.PaymentMethod` string column already stores `paymob_wallet`/`paymob_card`.

---

## 2. Enums, Entities & Pricing

### 2.1 AppointmentStatus `ClinicHub.Domain/Enums/AppointmentStatus.cs:3`
```csharp
Pending=0, Confirmed=1, Cancelled=2, Completed=3, Reserved=4, NoShow=5, Accepted=6, Rejected=7
```
Dashboard strings: `pending`, `reserved`, `accepted`/`awaiting-payment` (6), `confirmed`, `cancelled`, `rejected`, `completed`.

### 2.2 Appointment `ClinicHub.Domain/Entities/Appointment.cs:7`
```
BookedByUserId, DoctorId, ClinicId, AppointmentDate, StartTime, EndTime, AppointmentType, Status, PatientFullName/Age/Gender/Complaint/ChronicDiseases, CancellationReason, ExpiresAt, PaymentId→Payment
Methods: Reserve(ttl) → Reserved+ExpiresAt, Confirm(paymentId)→Confirmed, Accept()→Accepted, Reject(), Cancel(), Complete(), CheckIn(), MarkNoShow()
```

### 2.3 AppointmentType `ClinicHub.Domain/Enums/AppointmentType.cs:3`
```csharp
Examination=0 // كشف , FollowUp=1 // إعادة
```

### 2.4 PaymentType `ClinicHub.Domain/Enums/PaymentType.cs:3`
```csharp
Appointment=0, Subscription=1, Ads=2
```

### 2.5 PaymentStatus `ClinicHub.Domain/Enums/PaymentStatus.cs:3`
```csharp
Pending=0, Paid=1, Failed=2, Refunded=3, Processing=4
```
Webhook handles both `Pending` and `Processing` as unpaid. `Processing` → `Paid` is the normal transition.

### 2.6 PaymentMethod `ClinicHub.Domain/Enums/PaymentMethod.cs:3`
```csharp
Cash=0, PaymobWallet=1, PaymobCreditCard=2
```
Stored as `Payment.PaymentMethod` string (`ClinicHub.Domain/Entities/Payment.cs:27`): `cash` / `paymob_wallet` / `paymob_card` via `PaymentMethodMapper.cs:22` `ToDbString`.

### 2.7 Payment `ClinicHub.Domain/Entities/Payment.cs:7`
```
Type, Code="#P-xxxxxx", RefNumber="PM-YYYY-xxxxxx", AppointmentId?, UserId, ClinicId, SubscriptionId?, PlanId?, Amount, Currency="EGP", PaymobOrderId, PaymobTransactionId, Status, PaymentMethod, PaidAt, RedirectUrl, FailureReason, TransactionId, RefundedAt
Methods: MarkAsProcessing(url,method)→Processing, SetPaymobCheckout, MarkAsPaid(tx,method)→Paid, MarkAsManuallyPaid, MarkAsFailed, MarkAsRefunded
```

### 2.8 BookingConfiguration `ClinicHub.Domain/Entities/BookingConfiguration.cs:7` (per clinic)
```
ConsultationFee decimal (EGP), Currency="EGP", MaxAdvanceBookingDays=30, ReservationTtlMinutes=10, CancellationWindowMinutes=120
```
Accessed via `IBookingConfigurationRepository.GetByClinicIdAsync` + `ClinicHub.Application/Features/Appointments/Commands/CreateAppointment/CreateAppointmentCommandHandler.cs:46`.

### 2.9 Pricing `ClinicHub.Application/Common/AppointmentPricingCalculator.cs:10`
```csharp
platformFee = percent<=0 ? 0 : round(fee * percent/100, 2) // AwayFromZero
total       = fee + platformFee
```
`percent = PlatformSetting.AppointmentFeePercent` (single row, `GetPlatformFeePercentAsync` in handlers). Clinic keeps full `ConsultationFee`; patient pays `total`. Currency `EGP`; Paymob receives `total*100` cents (`PaymobService.cs:59,92`). Example: fee 200, platform 10% → patient pays **220 EGP** (`AppointmentAcceptanceService.cs:59`, `InitiateBookingPaymentCommandHandler.cs:57`, `InitiatePaymentCommandHandler.cs:55`).

### 2.10 PaymentMethodMapper `ClinicHub.Application/Features/AdminPayments/PaymentMethodMapper.cs:6`
```csharp
ToEnum(null|"") → PaymobWallet
"cash" → Cash
"wallet"|"paymob_wallet"|"paymob"|"paymobwallet" → PaymobWallet
"creditcard"|"card"|"credit_card"|"paymob_card"|"paymobcreditcard"|"paymob_creditcard"|"paymobcredit_card"|"visa"|"mastercard" → PaymobCreditCard
_ → PaymobWallet  // unknown falls to wallet
// case-insensitive, trims
ToDbString(Cash)→"cash", PaymobCreditCard→"paymob_card", _→"paymob_wallet"
ToUiStatus(Processing)→Pending
```

---

## 3. Paymob Settings & Integration IDs

### 3.1 Settings `ClinicHub.Application/Common/Options/PaymobSettings.cs:3`
```csharp
SecretKey, PublicKey, HmacSecret, IntegrationId, WalletIntegrationId, WebhookUrl, RedirectionUrl, ApiKey, BaseUrl="https://accept.paymob.com/api"
```
Registered via `IOptions<PaymobSettings>` in `PaymobService.cs:24`. Injected with `HttpClient`. `appsettings.json` is gitignored (`AGENTS.md:62`).

### 3.2 Dashboard Mapping (corrected 2026-08-28 per `Prompts/prompt.txt`)

| Integration ID | Channel | Gateway | Created | Dashboard label | ClinicHub use |
|---|---|---|---|---|---|
| **5671290** | `online` | `MIGS` (Mastercard VPC legacy) | 2026-05-17 | `wallets` + `بطاقة ائتمان` (credit card) | **Card** → `PaymobSettings.IntegrationId` → `PaymobService.cs:93` `InitiateCheckoutPaymentAsync` |
| **5690721** | `online_new` | `UIG` (Unified) | 2026-05-26 | `محفظة` (wallet) | **Wallet** → `PaymobSettings.WalletIntegrationId` → `PaymobService.cs:60` `InitiateWalletPaymentAsync` |
| **5690722** | — | — | — | — | **NOT FOUND** `404 detail="Integration ID does not exist"` — old invalid value before fix |

**Live test:** `POST https://accept.paymob.com/v1/intention/` with test keys
```
5671290 → 201 gateway_type=MIGS order_id=... redirection_url=post_pay  (card legacy, still works with Intention API UnifiedCheckout)
5690721 → 201 gateway_type=UIG  order_id=... redirection_url=https://.../api/v1/payments/result (wallet UIG)
5690722 → 404
```

**Fixed configs** (local, gitignored, not committed):
```json
// appsettings.Development.json:227-228 / appsettings.Test.json:215-216
"PaymobSettings": {
  "IntegrationId": "5671290",       // card VPC/MIGS
  "WalletIntegrationId": "5690721"  // wallet UIG
}
```
Root cause of "credit card intent shows wallet": IDs were swapped → card flow called wallet gateway → UnifiedCheckout showed phone input, not card form. Fixed by swapping + `PaymentMethodMapper` fix.

### 3.3 Resolved issues (`5167292`)
- `PaymobService.cs:34` `ResolveIntegrationId` — safe `int.TryParse`, logs error instead of crash on placeholder `YOUR_...`
- `PaymobService.cs:182` — logs Paymob body, extracts `detail`/`message`/`error` and surfaces in `BadRequestException` so client sees `Integration ID does not exist`.

---

## 4. Routes Summary

Prefix `api/v{version:apiVersion}` → `api/v1`. Routes defined in `ClinicHub.API/Routes/ApiRoutes.cs:4`.

| # | Purpose | Method | Route (ApiRoutes constant) | Controller | Auth |
|---|---|---|---|---|---|
| **A** | Create reservation / appointment (patient books slot) | `POST` | `Reservations.Create` `POST /api/v1/reservations` `ApiRoutes.cs:165` + `Appointments.Create` `POST /api/v1/appointments` (+ `CreateAdminDashboard` `POST /api/v1/admin/dashboard/appointments`) `ApiRoutes.cs:138` | `ReservationsController.cs:20` / `AppointmentsController.cs:78` | `Bearer` any authenticated |
| **B** | Pay reserved booking (new) | `POST` | `Payments.CreateBooking` `POST /api/v1/payments` `ApiRoutes.cs:181` | `PaymentsController.cs:40` | `Bearer` must be `BookedByUserId` |
| **C** | Pay generic (Pending/Reserved/Accepted) | `POST` | `Payments.Initiate` `POST /api/v1/payments/initiate` `ApiRoutes.cs:180` | `PaymentsController.cs:29` | `Bearer` owner |
| **D** | Poll payment status | `GET` | `Payments.GetStatus` `GET /api/v1/payments/status/{appointmentId}` `ApiRoutes.cs:184` | `PaymentsController.cs:107` | `Bearer` owner |
| **E** | Manual verify (fallback) | `POST` | `Payments.VerifyBooking` `POST /api/v1/payments/verify` `ApiRoutes.cs:182` | `PaymentsController.cs:51` | `Bearer` |
| **F** | My appointments (patient list with payment) | `GET` | `Appointments.My` `GET /api/v1/appointments/my?PageNumber=&PageSize=` `ApiRoutes.cs:141` | `AppointmentsController.cs:50` | `Bearer` |
| **G** | Available slots (before booking) | `GET` | `Slots.GetByDoctor` `GET /api/v1/clinics/{clinicId}/doctors/{doctorId}/slots?date=YYYY-MM-DD` + `GetByDoctorAdminDashboard` `ApiRoutes.cs:160` | `SlotsController.cs:17` | `Auth` |
| **H1** | Clinic admin accept → generate payment link | `PUT` | `Appointments.Accept` `PUT /api/v1/appointments/{id}/accept?paymentMethod=&returnUrl=` `ApiRoutes.cs:146` | `AppointmentsController.cs:122` | `ClinicOwner` (`RoleAuthorize`) |
| **H2** | Staff approve | `PUT` | `StaffDashboard.ApproveAppointment` `PUT /api/v1/staff/appointments/{id}/approve?paymentMethod=&returnUrl=` `ApiRoutes.cs:258` | `StaffController.cs:53` | `Staff` |
| **H3** | Doctor accept | `PUT` | `DoctorDashboard.AcceptAppointment` `PUT /api/v1/doctors/appointments/{id}/accept?paymentMethod=&returnUrl=` `ApiRoutes.cs:242` | `DoctorDashboardController.cs:66` | `Doctor` / `ClinicOwner` |
| **H4** | Doctor unified status (`status=6` = accept) | `PUT` | `DoctorDashboard.UpdateAppointmentStatus` `PUT /api/v1/doctors/appointments/{id}/status` `ApiRoutes.cs:241` | `DoctorDashboardController.cs:107` | `Doctor` |
| **W** | Webhook (Paymob → backend) | `POST` | `Payments.Webhook` `POST /api/v1/payments/webhook?hmac=` body `ConfirmPaymentWebhookRequestDto` `ApiRoutes.cs:183` | `PaymentsController.cs:88` | `AllowAnonymous` HMAC |
| **R** | Payment result redirect | `GET` | `Payments.Result` `GET /api/v1/payments/result?success=` `ApiRoutes.cs:185` | `PaymentsController.cs:137` | `AllowAnonymous` |
| **S** | Subscription verify (owner) | `GET` | `Payments.VerifyLatestSubscription` `GET /api/v1/payments/verify-latest-subscription` `ApiRoutes.cs:186` | `PaymentsController.cs:127` | `Bearer` |
| **Ads1** | Ad packages (owner) | `GET` | `Ads.Packages` `GET /api/v1/ads/packages` `ApiRoutes.cs:340` | `AdsController.cs:34` | `AllowAnonymous` (active only) |
| **Ads2** | My ads (owner) | `GET` | `Ads.MyAds` `GET /api/v1/clinics/{clinicId}/ads?status=` `ApiRoutes.cs:338` | `AdsController.cs:24` | `ClinicOwner` |
| **Ads3** | Create ad order (owner) | `POST` | `Ads.CreateOrder` `POST /api/v1/clinics/{clinicId}/ads/orders` `ApiRoutes.cs:339` | `AdsController.cs:46` | `ClinicOwner` |
| **Ads4** | Public active ads (patient app) | `GET` | `PublicAds.Active` `GET /api/v1/public/ads/active` `ApiRoutes.cs:344` | `PublicAdsController.cs:18` | `AllowAnonymous` |

Headers every call: `Authorization: Bearer <jwt>`, `Content-Type: application/json`, `Accept-Language: ar`.

---

## 5. Appointment Payment Workflow

### Step A — Create reservation / appointment

**Which endpoint:** `POST /reservations` (preferred for patient booking) or `POST /appointments`. Both use `CreateAppointmentCommand` → `CreateAppointmentCommandHandler.cs:44`.

**Logic:**
1. Load `BookingConfiguration` by `clinicId` (`BookingConfigurationRepository.GetByClinicIdAsync`). If null → `400 BookingConfigNotFound`.
2. Validate `appointmentDate+startTime > now` and `appointmentDate <= now+MaxAdvanceBookingDays` → else `400 PastDate/InvalidDate`.
3. Create `Appointment(userId, doctorId, clinicId, date, start, end, type, patientName, age, gender, complaint, chronicDiseases)` (`Appointment.cs:40`) with `Status=Pending`.
4. If `consultationFee > 0` → `appointment.Reserve(reservationTtlMinutes)` (`Appointment.cs:73`) → `Status=Reserved`, `ExpiresAt=now+ttl` (default 10 min). Else stays `Pending`.
5. `AppointmentRepository.AddAsync` + `SaveChanges` (commit before scheduling so job can query).
6. Schedule `ScheduleReservationExpirationAsync(appointmentId, expiresAt)` (`CreateAppointmentCommandHandler.cs:85`) — if Hangfire fails, logged warning and hourly sweep handles expiry.
7. `NotifyClinicStaffAsync` — FCM `NewBookingRequest` to doctor + clinic owner + all staff of clinic (`CreateAppointmentCommandHandler.cs:113`).
8. `SaveChanges` for notifications, map to `AppointmentDto` with `amount=consultationFee`, `currency`.

**Status after A:**
- Paid clinic (`fee>0`) → `Reserved` + `expiresAt` — **must pay via B within window** or auto `Cancelled`.
- Free clinic → `Pending` — waits for clinic accept (H) which creates payment.

### Step B — Pay reserved booking (patient-initiated, preferred)

**Endpoint:** `POST /payments` → `InitiateBookingPaymentCommandHandler.cs:28`

1. Load appointment by `reservationId`; if null → `404 ReservationNotFound`.
2. Check `BookedByUserId == currentUser` → else `401 Unauthorized`.
3. Check `IsReservationExpired()` (`Appointment.cs:70` `ExpiresAt && Reserved && now>=ExpiresAt`) → `409 ReservationExpired`.
4. Check `Status==Reserved` → else `400 AppointmentNotPending`.
5. Check existing `Payment` for appointment if `Paid` → `400 AlreadyPaid`.
6. Load `BookingConfiguration` + `PlatformFeePercent` → `amount = CalculateTotal(fee, percent)` (`AppointmentPricingCalculator.cs:15`), `currency`.
7. Load patient `ApplicationUser` → `CreateBillingData` (`InitiateBookingPaymentCommandHandler.cs:106`: split `FullName`, fallback `patient@clinichub.com` / `01000000000`, city `Cairo`, `NA` for address).
8. Resolve `paymentMethod`: (`InitiateBookingPaymentCommandHandler.cs:65`)
   ```csharp
   hasExplicit = !IsNullOrWhiteSpace(request.PaymentMethod)
   resolved = hasExplicit ? ToEnum(request.PaymentMethod) : PaymobCreditCard // legacy default = card
   if resolved==PaymobCreditCard → PaymobService.InitiateCheckoutPaymentAsync(amount,currency,billing,ct,returnUrl)
   else → InitiateWalletPaymentAsync(amount,currency,billing,phone,ct,returnUrl)
   ```
9. Create `Payment(Appointment, userId, clinicId, amount, currency)` with `PaymobOrderId=checkout.OrderId`, `LinkToAppointment`, `MarkAsProcessing(redirectUrl, paymob_card|paymob_wallet)` (`Payment.cs:66`), `SaveChanges`.
10. Return `BookingPaymentResponseDto` with `redirectUrl` (UnifiedCheckout).

### Step C — Pay generic (retry / Pending / Accepted)

**Endpoint:** `POST /payments/initiate` → `InitiatePaymentCommandHandler.cs:31`

1. Load appointment by `appointmentId`; null → `404 AppointmentNotFound`.
2. Check owner; wrong user → `401`.
3. Check `Status in (Pending, Reserved)` → else `400 AppointmentNotPending`.
4. Load `BookingConfiguration` + `PlatformFeePercent` → `amount`.
5. `billing = CreateBillingData(user)`.
6. Resolve: (`InitiatePaymentCommandHandler.cs:63`)
   ```csharp
   resolved = ToEnum(request.PaymentMethod) // null → PaymobWallet (backward compat for this route)
   if resolved==PaymobCreditCard → InitiateCheckoutPaymentAsync
   else → InitiateWalletPaymentAsync
   ```
7. Load existing `Payment` by appointment (`PaymentRepository.GetByAppointmentIdAsync`):
   - If exists and `Paid` → `400 AlreadyPaid`.
   - If exists (Pending/Processing/Failed) → update `PaymobOrderId`, `MarkAsProcessing(newRedirect, newMethod)`.
   - Else create new `Payment` + `LinkToAppointment` + `MarkAsProcessing`.
8. `SaveChanges`, return `InitiatePaymentResponseDto { paymentKey=clientSecret, redirectUrl, paymentId }`.

### Step H — Clinic accept → generates payment link

**Endpoints:** `PUT /appointments/{id}/accept`, `PUT /staff/appointments/{id}/approve`, `PUT /doctors/appointments/{id}/accept`, `PUT /doctors/appointments/{id}/status` (with `status:6`).

All delegate to `AppointmentAcceptanceService.AcceptAsync` (`ClinicHub.Application/Common/Services/AppointmentAcceptanceService.cs:41`) after auth check (`AppointmentsController.cs:34` clinic owner check, `DoctorDashboardController.cs:26` doctor role, `StaffController.cs:22` staff role).

`AcceptAsync`:
1. Guard `Status in (Pending, Reserved)` else `400 CannotRespondAppointment`.
2. Guard existing `Payment` if `Paid|Refunded` → `409 AlreadyAcceptedPayment`.
3. Load `BookingConfiguration`; if null or `fee<=0` → `400 FeeNotConfigured`.
4. `amount = CalculateTotal(fee, platformPercent)`, `currency`, `billing = CreateBillingData(patientUser)`.
5. Resolve method (`AppointmentAcceptanceService.cs:68`):
   ```csharp
   resolved = ToEnum(paymentMethod)
   hasExplicit = !IsNullOrWhiteSpace(paymentMethod)
   if hasExplicit && resolved==Wallet → InitiateWalletPaymentAsync
   else if hasExplicit && resolved==Card → InitiateCheckoutPaymentAsync
   else → InitiateCheckoutPaymentAsync // null → card (original behavior)
   dbMethod = ToDbString(hasExplicit ? ToEnum(paymentMethod) : PaymobCreditCard)
   ```
6. Create or refresh `Payment` with `PaymobOrderId`, `MarkAsProcessing(redirectUrl, dbMethod)`.
7. `appointment.Accept()` → `Accepted` (`Appointment.cs:99`), `SaveChanges`.
8. `ScheduleNoShowMarkingAsync(appointmentId, date+EndTime+30m)`.
9. FCM `AppointmentConfirmation` to patient with `paymentUrl=redirectUrl` + `AppointmentAccepted` to doctor+owner (`AcceptAsync:105,116`).
10. `SaveChanges` for notifications, return `AppointmentAcceptanceResultDto { appointmentId, status:6, paymentId, amount, currency, paymobRedirectUrl, paymobPaymentKey }`.

### Step W — Pay on Paymob, Webhook confirms

Patient opens `redirectUrl` (UnifiedCheckout) → Paymob handles wallet phone vs card form per `payment_methods=[integrationId]` → user completes → Paymob `POST /payments/webhook?hmac=...` → handler marks `Paid` → `Appointment` becomes `Confirmed`.

See §9.

### Step D/E/F — Poll & verify, My appointments

- **D** `GET /payments/status/{appointmentId}` → `GetPaymentStatusQueryHandler.cs:22` checks owner, returns `PaymentStatusDto { paymentId, appointmentId, status, amount, paidAt, transactionId }`.
- **E** `POST /payments/verify` `{ paymentId, transactionId }` → `VerifyBookingPaymentCommandHandler.cs:24`: if `Paid` → return current; if `Processing|Pending` → `MarkAsPaid(transactionId, storedMethod ?? "cash")`, `appointment.Confirm(paymentId)`, schedule `CancellationWindowClose(PaidAt+window)` + `NoShowMarking`, `SaveChanges`; if `Failed` → `400 PaymentFailed`.
- **F** `GET /appointments/my` → `GetMyAppointmentsQueryHandler` returns `PaginatedResult<MyAppointmentDto>` with `payment: { paymentId, amount, currency, paymentStatus, paymobRedirectUrl }` — `paymobRedirectUrl` present only while `Accepted` unpaid.

**Full state transitions:**
```
Create (fee>0) → Reserved (+expiresAt) ──B/C→ Processing (PaymobOrderId, redirectUrl) ──W/E→ Paid → Confirmed ──staff/doctor→ Completed / NoShow
Create (fee=0) → Pending ──H→ Accepted (Processing) ──W→ Confirmed
Any Reserved not paid before expiresAt → Cancelled (ReservationExpirationJob)
Confirmed + cancellation window → CancellationWindowClosed notification, becomes non-refundable
Confirmed past EndTime+30m unattended → NoShow (NoShowMarkingJob)
Rejected/Cancelled terminal
```

---

## 6. Request / Response Contracts

### 6.1 Create appointment `POST /api/v1/reservations` or `POST /api/v1/appointments`

**Request `CreateAppointmentCommand`:**
```json
{
  "doctorId": "guid",
  "clinicId": "guid",
  "appointmentDate": "2026-08-30",
  "startTime": "10:00:00",
  "endTime": "10:30:00",
  "appointmentType": 0,
  "patientFullName": "Ahmed Ali",
  "patientAge": 30,
  "patientGender": 0,
  "complaint": "headache",
  "chronicDiseases": null
}
```
`appointmentType`: `0 Examination`, `1 FollowUp`. `patientGender`: enum `Gender`.

**Response `201` `AppointmentDto`:**
```json
{
  "id": "apt-guid",
  "doctorId": "guid", "clinicId": "guid",
  "appointmentDate": "2026-08-30", "startTime": "10:00:00", "endTime": "10:30:00",
  "status": 4,
  "statusName": "Reserved",
  "expiresAt": "2026-08-29T14:40:00",
  "amount": 200,
  "currency": "EGP",
  "paymentId": null,
  "paymentUrl": null
}
```
`status` values §2.1. `amount` is `ConsultationFee` (not total) at this stage. If `Reserved`, `expiresAt` is deadline for B.

### 6.2 Pay reserved `POST /api/v1/payments`

**Request `InitiateBookingPaymentCommand`:**
```json
{
  "reservationId": "apt-guid-from-A",
  "paymentMethod": "wallet",
  "returnUrl": "myapp://payment-result"
}
```
`paymentMethod` values §7. Omitted → `card` for this route (legacy). `returnUrl` overrides `PaymobSettings.RedirectionUrl` for this intention only.

**Response `200` `BookingPaymentResponseDto` `ClinicHub.Application/Features/Payment/DTOs/BookingPaymentResponseDto.cs:5`:**
```json
{
  "paymentId": "pay-guid",
  "reservationId": "apt-guid",
  "amount": 220.00,
  "currency": "EGP",
  "status": 4,
  "transactionId": null,
  "redirectUrl": "https://accept.paymob.com/unifiedcheckout/?publicKey=egy_pk_test_...&clientSecret=xxx",
  "failureReason": null,
  "createdAt": "2026-08-29T14:30:00Z",
  "completedAt": null,
  "receiptUrl": null
}
```
`status=4 Processing` → webhook flips to `1 Paid`. `amount` is total (fee+platform). Do **not** build URL client-side; `publicKey` injected server-side.

**Errors:** `404 ReservationNotFound`, `409 ReservationExpired` (`IsReservationExpired`), `400 AppointmentNotPending` (not `Reserved`), `400 AlreadyPaid`, `400 BookingConfigNotFound`, `400 PaymobOrderFailed: Integration ID does not exist` (now surfaces detail).

### 6.3 Pay generic `POST /api/v1/payments/initiate`

**Request `InitiatePaymentCommand` `InitiatePaymentCommand.cs:6`:**
```json
{
  "appointmentId": "apt-guid",
  "paymentMethod": "card",
  "returnUrl": "myapp://payment-result"
}
```
Omitted `paymentMethod` → `wallet` for this route (old mobile builds).

**Response `200` `InitiatePaymentResponseDto` `InitiatePaymentResponseDto.cs:3`:**
```json
{
  "paymentId": "pay-guid",
  "paymentKey": "client_secret_from_paymob",
  "redirectUrl": "https://accept.paymob.com/unifiedcheckout/?publicKey=...&clientSecret=..."
}
```
Stored `PaymentMethod=paymob_wallet|paymob_card`, `Status=Processing`.

**Errors:** `404 AppointmentNotFound`, `401 Unauthorized` (not `BookedByUserId`), `400 AppointmentNotPending`, `400 AlreadyPaid`.

### 6.4 Accept `PUT /api/v1/appointments/{id}/accept?paymentMethod=&returnUrl=`

Query overrides JSON body. Body (optional, `EmptyBodyBehavior.Allow`):

```json
{ "paymentMethod": "card", "returnUrl": "myapp://result" }
```

Same for `PUT /staff/appointments/{id}/approve`, `PUT /doctors/appointments/{id}/accept`, and `PUT /doctors/appointments/{id}/status`:

```json
// PUT /doctors/appointments/{id}/status
{ "status": 6, "paymentMethod": "wallet", "returnUrl": "myapp://result", "notes": null }
```
`status` unified: `1=Accept`, `2=Reject`, `3=Complete` (`DoctorDashboardController.cs:102`).

**Response `200` `AppointmentAcceptanceResultDto`:**
```json
{
  "appointmentId": "guid",
  "status": 6,
  "paymentId": "guid",
  "amount": 220.00,
  "currency": "EGP",
  "paymobRedirectUrl": "https://accept.paymob.com/unifiedcheckout/?publicKey=...&clientSecret=...",
  "paymobPaymentKey": "client_secret"
}
```
Wrapped in `ApiResponse` with message `AcceptedWithPaymentLink`. `amount` includes platform fee.

**Errors:** `404 AppointmentNotFound`, `400 NotAuthorizedToRespond` / `CannotRespondAppointment`, `409 AlreadyAcceptedPayment`, `400 FeeNotConfigured`.

### 6.5 Get payment status `GET /api/v1/payments/status/{appointmentId}`

**Response `200` `PaymentStatusDto` `PaymentStatusDto.cs:5`:**
```json
{ "paymentId": "pay-guid", "appointmentId": "apt-guid", "status": 1, "amount": 220.00, "paidAt": "2026-08-29T14:35:00", "transactionId": "123456" }
```
`status` enum §2.5. Owner check enforced (`GetPaymentStatusQueryHandler.cs:28`).

### 6.6 Verify booking `POST /api/v1/payments/verify`

**Request:**
```json
{ "paymentId": "pay-guid", "transactionId": "paymob_tx_or_cash_ref" }
```
**Response `200` `BookingPaymentResponseDto`** with `status:1 Paid` and `completedAt`. If already `Paid` returns current. If `Failed` → `400 PaymentFailed`.

### 6.7 My appointments `GET /api/v1/appointments/my?PageNumber=1&PageSize=20`

**Response `200` `PaginatedResult<MyAppointmentDto>`:**
```json
{
  "items": [{
    "id": "apt-guid",
    "clinicId": "guid", "clinicName": "Al-Noor",
    "doctorId": "guid", "doctorName": "Dr. Samir",
    "appointmentDate": "2026-08-30", "startTime": "10:00", "endTime": "10:30",
    "status": 6, "statusName": "Accepted",
    "payment": {
      "paymentId": "pay-guid",
      "amount": 220.00,
      "currency": "EGP",
      "paymentStatus": 0,
      "paymobRedirectUrl": "https://accept.paymob.com/unifiedcheckout/?publicKey=...&clientSecret=..."
    }
  }],
  "pageNumber": 1, "pageSize": 20, "totalCount": 5
}
```
`paymobRedirectUrl` present only while `Accepted` & unpaid → show "Pay now".

### 6.8 Slots `GET /api/v1/clinics/{clinicId}/doctors/{doctorId}/slots?date=YYYY-MM-DD`

**Response `200` `GetAvailableSlotsResponse`:** list of `{ slotId, startTime, endTime, isAvailable }` from `GetAvailableSlotsQuery`.

### 6.9 Webhook `POST /api/v1/payments/webhook?hmac=...`

**Request `ConfirmPaymentWebhookRequestDto`:**
```json
{
  "type": "TRANSACTION",
  "hmac": "optional-also-query",
  "transaction": {
    "id": 123456,
    "amount_cents": 22000,
    "currency": "EGP",
    "success": true,
    "pending": false,
    "is_auth": false, "is_capture": true, "is_standalone_payment": true,
    "is_voided": false, "is_refunded": false, "is_3d_secure": true,
    "error_occured": false, "has_parent_transaction": false,
    "integration_id": 5671290,
    "profile_id": 123, "owner": 123,
    "created_at": "2026-08-29T14:35:00",
    "order": { "id": 987654 },
    "source_data": { "pan": "****1234", "sub_type": "MasterCard", "type": "card" }
  }
}
```
Mobile **never** calls this. Paymob calls it; validation via `PaymobService.cs:216`.

**Response `200` `true|false`** (idempotent `true` for duplicates, `false` for HMAC fail or missing order).

### 6.10 Payment result `GET /api/v1/payments/result?success=true`

Redirects to `/payment/result.html?success=true|false`. `PaymentsController.cs:137`. Use as `returnUrl` fallback if not using deep link.

---

## 7. Credit vs Wallet Branching

| Input `paymentMethod` | Normalized | Enum | Integration | Service method | DB string | Display |
|---|---|---|---|---|---|---|
| `null`/empty on **B** (`/payments`) | default card | `PaymobCreditCard` | `5671290` MIGS | `InitiateCheckoutPaymentAsync` `PaymobService.cs:85` | `paymob_card` | بطاقة |
| `null`/empty on **C** (`/payments/initiate`) | default wallet | `PaymobWallet` | `5690721` UIG | `InitiateWalletPaymentAsync` `PaymobService.cs:51` | `paymob_wallet` | محفظة |
| `null`/empty on **H** (accept) | default card | `PaymobCreditCard` | `5671290` | `InitiateCheckoutPaymentAsync` `AppointmentAcceptanceService.cs:74` | `paymob_card` | بطاقة |
| `"wallet"` `"paymob_wallet"` `"paymob"` `"paymobwallet"` | wallet | `PaymobWallet` | `5690721` | `InitiateWalletPaymentAsync` | `paymob_wallet` | محفظة |
| `"card"` `"creditcard"` `"credit_card"` `"paymob_card"` `"paymobcreditcard"` `"paymob_creditcard"` `"visa"` `"mastercard"` | card | `PaymobCreditCard` | `5671290` | `InitiateCheckoutPaymentAsync` | `paymob_card` | بطاقة |
| `"cash"` | cash | `Cash` | — (manual `MarkAsManuallyPaid`) | — | `cash` | نقدي |

All matching is `Trim().ToLowerInvariant()` (`PaymentMethodMapper.cs:12`). **Always send explicit `paymentMethod:"card"|"wallet"`** to avoid default confusion between B (card) and C (wallet).

Paymob UnifiedCheckout UI per `payment_methods=[integrationId]`:
- Wallet `5690721` → phone input (Vodafone Cash / Orange / Etisalat / Meeza wallet)
- Card `5671290` → pan / expiry / cvv (Mastercard/Visa, 3D Secure)

---

## 8. Paymob Service Internals

### 8.1 Intention API `PaymobService.cs:117` `CreateIntentionAsync`

Single call replacing legacy `auth → order → payment_key` flow:

```
POST https://accept.paymob.com/v1/intention/
Authorization: Token {SecretKey}
Content-Type: application/json
{
  "amount": 22000,               // amount*100 int cents
  "currency": "EGP",
  "payment_methods": [5671290],   // or [5690721]
  "items": [{ "name":"ClinicHub Appointment", "amount":22000, "description":"Medical appointment booking", "quantity":1 }],
  "billing_data": {
    "first_name": "Ahmed", "last_name":"Ali",
    "email":"patient@clinichub.com", "phone_number":"01000000000", // ToPaymobFormat()
    "apartment":"NA","floor":"NA","street":"NA","building":"NA",
    "postal_code":"NA","city":"Cairo","country":"EG","state":"Cairo"
  },
  "notification_url": "https://.../api/v1/payments/webhook", // WebhookUrl
  "redirection_url": "https://.../api/v1/payments/result"    // RedirectionUrl or per-request returnUrl
}
→ 201 { "client_secret":"...", "intention_order_id": 123456 } or { "id":"..." }
→ redirectUrl = "https://accept.paymob.com/unifiedcheckout/?publicKey={PublicKey}&clientSecret={client_secret}"
```

`CreateIntentionAsync` parses `client_secret` and `intention_order_id`/`id` (`PaymobService.cs:198`). Errors logged (`PaymobService.cs:184`) and thrown as `BadRequestException` with localized `PaymobOrderFailed` + extracted `detail`.

### 8.2 HMAC Validation `PaymobService.cs:216` `ValidateWebhookAsync`

Concatenates 19 fields in Paymob-specified order:

```
amount_cents + created_at + currency + error_occured + has_parent_transaction + id + integration_id + is_3d_secure + is_auth + is_capture + is_refunded + is_standalone_payment + is_voided + order.id + owner + pending + source_data.pan + source_data.sub_type + source_data.type + success
```
All lowercased booleans, `owner` falls back to `profileId` if `owner==0`. Computed `HMAC-SHA512(HmacSecret, concatenated)` (`PaymobService.cs:343`) compared case-insensitive to `hmac` query/body. Uses `SHA512` (not SHA256) per Paymob spec. `GetValue` tries both `order.id` and `order_id` variants.

### 8.3 Order Inquiry `PaymobService.cs:260` `GetOrderPaymentStatusAsync`

Best-effort fallback for subscriptions (`PaymentsController.cs:127` `VerifyLatestSubscriptionPayment`). Legacy flow: `POST /api/auth/tokens {api_key}` → token → `GET /api/ecommerce/orders/{orderId}?token=...` → `paid_amount_cents >= amount_cents` → `Paid`. Failures return `Found=false` (caller falls back to webhook truth). No appointment equivalent — use D/E polling.

### 8.4 Refund `PaymobService.cs:351` `RefundTransactionAsync`

```
POST https://accept.paymob.com/api/acceptance/void_refund/refund
Authorization: Token {SecretKey}
{ "transaction_id": 123, "amount_cents": 22000 }
```
Used via `PaymentRefundGate` for `AdminPayments` refund flow.

### 8.5 Error Diagnostics `PaymobService.cs:34` `ResolveIntegrationId`

```csharp
candidate = !IsNullOrWhiteSpace(rawId) ? rawId : fallbackRaw
if !int.TryParse(candidate) → log error + throw BadRequestException("Paymob IntegrationId '{candidate}' is not configured.")
```
Prevents crash on placeholder `YOUR_...` in `appsettings.Production.json`.

---

## 9. Webhook — Server Truth & Idempotency

**Handler:** `ClinicHub.Application/Features/Payment/Commands/ConfirmPaymentWebhook/ConfirmPaymentWebhookCommandHandler.cs:33`

```csharp
if Type != "TRANSACTION" → true (ignore)
if empty hmac → log + false
if transaction==null || order.id==0 → log + false
transactionData = TransactionToDictionary(transaction) // 19 fields
if !ValidateWebhookAsync(hmac, data) → log + false

payment = PaymentRepository where PaymobOrderId == order.id.ToString() // first match
if null → log + false
if payment.Status in (Paid, Refunded) → true (idempotent skip) // Failed NOT skipped → retry allowed
if transaction.Success:
    payment.MarkAsPaid(txId, source_data.sub_type ?? "Unknown") // Payment.cs:80 Paid+PaidAt+TransactionId
    if Type==Appointment && AppointmentId != null:
        appointment = AppointmentRepository where Id == payment.AppointmentId + Include Clinic
        appointment.Confirm(payment.Id) // Appointment.cs:85 Confirmed+ExpiresAt=null
        try {
          FcmService.SendToUserAsync(patient, PaymentConfirmation {amount, appointmentId})
          NotifyClinicOwnerAndSuperAdmins(appointment, payment) // PaymentReceived + RevenueIncreased
          bookingConfig = BookingConfigurationRepository.GetByClinicIdAsync(clinicId)
          ScheduleCancellationWindowCloseAsync(appointmentId, PaidAt + window) // default 120m
          ScheduleNoShowMarkingAsync(appointmentId, Date+EndTime+30m)
        } catch → log non-critical, still confirm
    else if Ads → Advertisement.Activate(now, duration) + ScheduleAdExpiration
    else if Subscription → SubscriptionPaymentCompleter.ActivateFromPaymentAsync
else:
    payment.MarkAsFailed() // Payment.cs:99 Failed
SaveChanges → true
```

**Notifications:** `NotifyClinicOwnerAndSuperAdminsAsync` sends `PaymentReceived` to `Clinic.ClinicAdminId` and `RevenueIncreased` to all `SuperAdmin` users (summing paid appointment revenue + current). `TransactionToDictionary` maps `order.id`, `owner`, `source_data.*`, booleans lowercased.

**Mobile rule:** Never trust client `success=true` redirect. Only `GET /payments/status` or webhook `Paid` is truth. Poll D after WebView close; webhook may arrive seconds later.

---

## 10. Clinic Accept Flows

All accept paths now accept `paymentMethod` + `returnUrl` via query or JSON body; query overrides body.

| Path | Route | Handler | Auth check |
|---|---|---|---|
| Clinic admin | `PUT /appointments/{id}/accept` `AppointmentsController.cs:122` (`EmptyBodyBehavior.Allow`) | `AcceptAppointmentCommandHandler.cs:28` checks `Clinic.ClinicAdminId==currentUser` → `AcceptAsync` | `RoleAuthorize` clinic owner |
| Staff | `PUT /staff/appointments/{id}/approve` `StaffController.cs:53` | `StaffApproveAppointmentCommandHandler` → `AcceptAsync` | `Staff` + `ManageAppointments` |
| Doctor | `PUT /doctors/appointments/{id}/accept` `DoctorDashboardController.cs:66` | `DoctorAcceptAppointmentCommandHandler` → `AcceptAsync` | `Doctor` |
| Doctor unified | `PUT /doctors/appointments/{id}/status` `DoctorDashboardController.cs:107` body `{status:6}` | `UpdateAppointmentStatusCommandHandler` forwards `paymentMethod` on `Accept` case | `Doctor` |

After accept, `F` shows `paymobRedirectUrl` → patient taps "Pay now" → same UnifiedCheckout → same webhook.

---

## 11. Ads Payments (Clinic Owner)

Independent from subscriptions since this release — only needs `Clinic.Status==Active` (was `subscription.Status==Active`). `AdsOrderProcessor.cs:123` now `return clinic != null && clinic.Status==Active`.

**Endpoints:**

| Purpose | Route | Auth | Request | Response |
|---|---|---|---|---|
| Packages | `GET /ads/packages` `AdsController.cs:34` | Anonymous | — | `AdPackageDto[] {id,name,nameAr,price,durationDays,isActive}` |
| My ads | `GET /clinics/{clinicId}/ads?status=` `AdsController.cs:24` | ClinicOwner | — | `AdDto[] {id,packageId,packageNameAr,durationDays,amount,currency,status, startDate,endDate,createdAt,imageUrl}` |
| Create order | `POST /clinics/{clinicId}/ads/orders` `AdsController.cs:46` | ClinicOwner | `{adPackageId:guid, durationDays:int, logoImageUrl?:string, paymentMethod?:"wallet"|"card", returnUrl?:string}` | `201 {paymentId,refNumber,amount,currency,status:4, paymobRedirectUrl, paymobPaymentKey, imageUrl}` |
| Public ads | `GET /public/ads/active` `PublicAdsController.cs:18` | Anonymous | — | `PublicAdDto[] {id,clinicId,clinicName,clinicLogoUrl,imageUrl,packageId,packageNameAr,title,startDate,endDate}` |

**Contract:** `logoImageUrl` is relative file name from `POST /attachments/upload?place=5` (multipart `file`) → `{base}/files/{imageUrl}`. `durationDays` must be multiple of package `durationDays`. Only webhook (`Paid` → `Advertisement.Activate`) or superadmin cash flips `0 PendingPayment → 1 Active` (`startDate=now, endDate=now+durationDays`); polling `GET /clinics/{clinicId}/ads` reflects status. Expired auto-excluded from public endpoint. Mobile **only reads** public endpoint (no create/payment); carousel contract:

```
if imageUrl != null: Image(src="{base}/files/{imageUrl}", fallback=clinicName)
else: TextBadge { clinicName, packageNameAr, startDate→endDate }
Tag: "نشط"  Tap → navigate clinicId
```

Refresh on app open + pull-to-refresh. Payment method same mapper: `AdsOrderProcessor.cs:72` `ToEnum(paymentMethod)` → `InitiateCheckoutPaymentAsync` vs `InitiateWalletPaymentAsync`.

**Statuses:** `0 PendingPayment` / `1 Active` / `2 Expired` / `3 Deactivated`.

---

## 12. Subscription Parity

`POST /subscriptions/initiate-payment` body `{PlanId:guid, Period:0 Monthly|1 Yearly, PaymentMethod?:"wallet"|"card", ReturnUrl?}` → same mapper → same Intention API → `InitiateSubscriptionPaymentResponseDto {PaymentId, PaymobRedirectUrl, ...}`. Verify via `GET /payments/verify-latest-subscription` (`PaymentsController.cs:127`) which does `GetOrderPaymentStatusAsync` legacy token inquiry. Webhook activates via `SubscriptionPaymentCompleter`.

---

## 13. Mobile WebView, Polling & Deep Links

1. Call **B** or **C** (or get `paymobRedirectUrl` from **F** after **H**) → store `paymentId` + `redirectUrl`.
2. Open `redirectUrl` in `WebView` (`WKWebView` / `WebView` / `CustomTabs`). Do **not** construct URL — `publicKey` injected server-side.
3. Paymob shows wallet phone vs card form per `payment_methods`.
4. Intercept navigation:
   - URL contains `success=true` → close WebView, poll **D**.
   - `success=false` → show retry (re-initiate B/C with same appointment + other `paymentMethod`).
5. Poll `GET /payments/status/{appointmentId}` every 3s up to 60s (or until `Paid`). Then `GET /appointments/my` shows `Confirmed`.
6. Fallback: `POST /payments/verify` with `{paymentId, transactionId}` from redirect query if webhook delayed. For subscriptions, `GET /payments/verify-latest-subscription`.

**Deep links / Redirection:** `PaymobSettings.RedirectionUrl`/`returnUrl` can be `myapp://payment-result?success=...` (intercept via `DeepLinkRoutes` `ClinicHub.Application/Common/DeepLinkRoutes.cs`) or `https://{host}/go/{path}` (`DeepLinksController.cs:374` `GoRoute`). Backend `GET /payments/result?success=` redirects to `/payment/result.html?success=` (`PaymentsController.cs:137`). Configure per-request `returnUrl` to your deep link.

**Never trust client success** — webhook is truth. `WebhookUrl` must point to `POST /payments/webhook?hmac=`.

---

## 14. Error Mapping

All errors wrapped in `ApiResponse<T>` (`ClinicHub.Application/Common/Models/ApiResponse.cs`): `{ isSuccess:false, errors:[{message,code}], data:null }` (`ApiExceptionFilterAttribute`).

| HTTP | Key (Localization) | Arabic (default) | Action |
|---|---|---|---|
| 400 | `Payments.AppointmentNotPending` `PaymentMessages.AppointmentNotPending` | الموعد غير قابل للدفع حالياً | Show retry / re-book |
| 400 | `Payments.AlreadyPaid` | تم الدفع مسبقاً | Show success |
| 400 | `Payments.AlreadyAcceptedPayment` | تم قبول الموعد مسبقاً | Refresh |
| 400 | `Payments.PaymentFailed` | فشل الدفع | Retry with other method |
| 400 | `Booking.FeeNotConfigured` | رسوم الحجز غير مهيأة | Contact clinic admin |
| 400 | `Appointments.CannotRespondAppointment` | لا يمكن قبول الموعد في حالته الحالية | Refresh list |
| 400 | `Booking.BookingConfigNotFound` | إعدادات الحجز غير موجودة | — |
| 400 | `Payments.PaymobOrderFailed` / `PaymobKeyFailed` | فشل إنشاء رابط الدفع — now surfaces Paymob `detail` | Check IDs per §3 |
| 404 | `Payments.AppointmentNotFound` / `Booking.ReservationNotFound` | غير موجود | Re-book |
| 404 | `Payments.NotFound` (status query, no payment) | غير موجود | No payment yet |
| 401 | `Payments.Unauthorized` | غير مصرح — نفس حساب الحجز | Re-login as booker |
| 409 | `Booking.ReservationExpired` | انتهت مهلة الحجز (10 دقائق) | Re-book new slot |
| 403 | `Booking.ReservationExpired` variant / slot taken | — | `GET /slots?date` → re-book |

`GET /payments/status` and `POST /payments/verify` are idempotent — safe to retry. `Failed` payments are **not** idempotent-skipped in webhook → same `PaymobOrderId` can be retried after failure with new intention.

---

## 15. FCM Notifications

| # | Type | Recipients | Trigger | `data` keys |
|---|---|---|---|---|
| 12 | `NewBookingRequest` | Doctor + clinic owner + staff matching `ClinicId` | `CreateAppointment` `CreateAppointmentCommandHandler.cs:145` | `patientName,clinicName,doctorName,date,time,appointmentId` |
| 3 | `AppointmentConfirmation` | Patient `BookedByUserId` | `AcceptAsync` `AppointmentAcceptanceService.cs:105` | `clinicName,date,appointmentId,paymentUrl` |
| 19 | `AppointmentAccepted` | Doctor + clinic owner | Same accept | `patientName,clinicName,doctorName,date,time,appointmentId` |
| 2 | `PaymentConfirmation` | Patient | Webhook `ConfirmPaymentWebhookCommandHandler.cs:95` | `amount,appointmentId` |
| 17 | `PaymentReceived` | Clinic owner `ClinicAdminId` | Webhook | `amount,patientName,clinicName,appointmentId` |
| 18 | `RevenueIncreased` | All `SuperAdmin` | Webhook | `amount,clinicName,totalRevenue,appointmentId` |

See `docs/NOTIFICATIONS_README.md` predecessors (now merged here) for all 0-19 types. Notifications persisted in `dbo.Notifications` via `NotificationBuilderService` and pushed via `FcmService` per `UserFbTokens`.

Mobile: listen for `PaymentConfirmation` (2) + `AppointmentConfirmation` (3) to auto-refresh. Web dashboard bell uses `GET /notifications/pagginated` + `GET /notifications/count`.

---

## 16. Background Jobs

Hangfire (`ClinicHub.Infrastructure/Services/BackgroundJobs/`):

| Job | Schedule | What it does |
|---|---|---|
| `ScheduleReservationExpirationAsync(id, expiresAt)` | One-shot at `ExpiresAt` (`CreateAppointmentCommandHandler.cs:85`) | `Status Reserved && now>=ExpiresAt` → `Cancelled` + FCM `AppointmentCancellation` |
| `ReservationExpirationJob` (hourly sweep) | Recurring hourly | Sweeps any missed `Reserved` past `ExpiresAt` |
| `ScheduleCancellationWindowCloseAsync(id, PaidAt+window)` | One-shot (`ConfirmPaymentWebhookCommandHandler.cs:106` + `VerifyBookingPayment:62`) | Notifies `CancellationWindowClosed` when refund window ends |
| `ScheduleNoShowMarkingAsync(id, Date+EndTime+30m)` | One-shot (`AcceptAsync:102` + `Confirm:65`) | `Confirmed` past scheduled end → `NoShow` |
| `ExpiryReminderJob` | Daily | `SubscriptionExpiring` / `AdExpiring` to owner |
| `AdExpiration` | One-shot per ad `endDate` | Deactivates expired ads |

Hangfire server: `WorkerCount=2`, dashboard at `/hangfire` (Dev: local only, Prod: SuperAdmin), health at `/health`. See `IBackgroundJobScheduler`.

---

## 17. cURL Examples

```bash
base=https://api.clinichub.example
jwt="Bearer <patient_jwt>"
clinicId="2d1a0000-0000-0000-0000-000000000003"
doctorId="3f9a0000-0000-0000-0000-000000000001"

# Slots
curl "$base/api/v1/clinics/$clinicId/doctors/$doctorId/slots?date=2026-08-30" -H "Authorization: $jwt"

# A. Create reservation (patient)
curl -X POST "$base/api/v1/reservations" \
  -H "Authorization: $jwt" -H "Content-Type: application/json" \
  -d "{\"clinicId\":\"$clinicId\",\"doctorId\":\"$doctorId\",\"appointmentDate\":\"2026-08-30\",\"startTime\":\"10:00:00\",\"endTime\":\"10:30:00\",\"appointmentType\":0,\"patientFullName\":\"Ahmed Ali\",\"patientAge\":30,\"patientGender\":0,\"complaint\":\"headache\"}"
# → 201 { id: "apt-guid", status:4, expiresAt:"..." }

apt="apt-guid-from-above"

# B. Pay reserved — wallet
curl -X POST "$base/api/v1/payments" \
  -H "Authorization: $jwt" -H "Content-Type: application/json" \
  -d "{\"reservationId\":\"$apt\",\"paymentMethod\":\"wallet\",\"returnUrl\":\"myapp://payment-result\"}"
# → 200 { paymentId, redirectUrl }

# C. Pay generic — card (retry / Pending / Accepted)
curl -X POST "$base/api/v1/payments/initiate" \
  -H "Authorization: $jwt" -H "Content-Type: application/json" \
  -d "{\"appointmentId\":\"$apt\",\"paymentMethod\":\"card\",\"returnUrl\":\"myapp://payment-result\"}"
# → 200 { paymentKey, redirectUrl, paymentId }

# D. Poll
curl "$base/api/v1/payments/status/$apt" -H "Authorization: $jwt"

# E. Verify (manual)
pay="pay-guid"
curl -X POST "$base/api/v1/payments/verify" \
  -H "Authorization: $jwt" -H "Content-Type: application/json" \
  -d "{\"paymentId\":\"$pay\",\"transactionId\":\"123456\"}"

# F. My appointments
curl "$base/api/v1/appointments/my?PageNumber=1&PageSize=20" -H "Authorization: $jwt"

# H. Accept (clinic dashboard) — card
clinicJwt="Bearer <clinic_owner_jwt>"
curl -X PUT "$base/api/v1/appointments/$apt/accept?paymentMethod=card" \
  -H "Authorization: $clinicJwt"

# H. Accept via body
curl -X PUT "$base/api/v1/appointments/$apt/accept" \
  -H "Authorization: $clinicJwt" -H "Content-Type: application/json" \
  -d '{"paymentMethod":"wallet","returnUrl":"myapp://payment-result"}'

# H. Staff approve — wallet
curl -X PUT "$base/api/v1/staff/appointments/$apt/approve?paymentMethod=wallet" \
  -H "Authorization: Bearer <staff_jwt>"

# H. Doctor accept — card
curl -X PUT "$base/api/v1/doctors/appointments/$apt/accept?paymentMethod=card" \
  -H "Authorization: Bearer <doctor_jwt>"

# Public ads (patient app, no auth)
curl "$base/api/v1/public/ads/active"

# Ads — create order (clinic owner)
adPkg="pkg-guid"
curl -X POST "$base/api/v1/clinics/$clinicId/ads/orders" \
  -H "Authorization: $clinicJwt" -H "Content-Type: application/json" \
  -d "{\"adPackageId\":\"$adPkg\",\"durationDays\":14,\"logoImageUrl\":\"clinic-logo-8f3a.png\",\"paymentMethod\":\"card\",\"returnUrl\":\"https://myhost/Clinic/AdPaymentResult\"}"
```

---

## 18. Changed Files

| File | Change |
|---|---|
| `ClinicHub.Application/Features/Payment/Commands/InitiatePayment/InitiatePaymentCommand.cs:6` | `record (Guid AppointmentId, string? ReturnUrl, string? PaymentMethod)` |
| `ClinicHub.Application/Features/Payment/Commands/InitiatePayment/InitiatePaymentCommandHandler.cs:31` | branch on `ToEnum(paymentMethod)` → wallet (`5690721`) vs card (`5671290`), `MarkAsProcessing(paymob_wallet|paymob_card)` |
| `ClinicHub.Application/Features/Payment/Commands/InitiateBookingPayment/InitiateBookingPaymentCommand.cs:8` | added `PaymentMethod?` + `ReturnUrl?` |
| `ClinicHub.Application/Features/Payment/Commands/InitiateBookingPayment/InitiateBookingPaymentCommandHandler.cs:28` | explicit handling (omitted→card), Paymob branching, `MarkAsProcessing` |
| `ClinicHub.Application/Features/Appointments/Commands/AcceptAppointment/AcceptAppointmentCommand.cs:8` | added `PaymentMethod?` + `ReturnUrl?` |
| `ClinicHub.Application/Features/Appointments/Commands/AcceptAppointment/AcceptAppointmentCommandHandler.cs:28` | forward to `AcceptAsync(...,paymentMethod,returnUrl)` |
| `ClinicHub.Application/Common/Interfaces/IAppointmentAcceptanceService.cs:14` | `AcceptAsync(..., string? paymentMethod, string? returnUrl)` |
| `ClinicHub.Application/Common/Services/AppointmentAcceptanceService.cs:41` | wallet/card branch, platform fee `CalculateTotal`, `ToDbString` stored, FCM |
| `ClinicHub.Application/Features/DoctorDashboard/Commands/DoctorAcceptAppointment/DoctorAcceptAppointmentCommand.cs:8` | added `PaymentMethod?` + `ReturnUrl?` + handler forward |
| `ClinicHub.Application/Features/StaffDashboard/Commands/StaffApproveAppointment/StaffApproveAppointmentCommand.cs:8` | same |
| `ClinicHub.Application/Features/DoctorDashboard/Commands/UpdateAppointmentStatus/UpdateAppointmentStatusCommand.cs:14` | added `PaymentMethod?` + `ReturnUrl?` (status=6) |
| `ClinicHub.Application/Features/DoctorDashboard/Commands/UpdateAppointmentStatus/UpdateAppointmentStatusCommandHandler.cs:53` | forward method on Accept case |
| `ClinicHub.Application/Features/AdminPayments/PaymentMethodMapper.cs:6` | extended `ToEnum` + `ToDbString` |
| `ClinicHub.API/Controllers/Version1/AppointmentsController.cs:122` | `Accept` query `paymentMethod`+`returnUrl` + `EmptyBodyBehavior.Allow` body |
| `ClinicHub.API/Controllers/Version1/DoctorDashboardController.cs:66` | `AcceptAppointment` same |
| `ClinicHub.API/Controllers/Version1/StaffController.cs:53` | `ApproveAppointment` same |
| `ClinicHub.Infrastructure/Services/Paymob/PaymobService.cs:34` | `ResolveIntegrationId` + `ILogger` + detail surfacing (`5167292`) |
| `ClinicHub.Application/Features/Ads/AdsOrderProcessor.cs:72,123` | `PaymentMethod` param + eligibility `Active` clinic only |
| `docs/README.md` (this file) | replaces 23 fragmented docs |

No migration — `Payment.PaymentMethod` string already exists. Solution `.slnx` clean architecture: `Domain ← Application ← Persistence → Infrastructure → API` (`AGENTS.md`).

---

## 19. Checklists

### Mobile patient app
- [ ] `GET /slots?date` → `POST /reservations` → if `Reserved` show method selector (محفظة / بطاقة) → `POST /payments {reservationId, paymentMethod, returnUrl:"myapp://payment-result"}` store `paymentId`+`redirectUrl`
- [ ] `WebView` `redirectUrl` (UnifiedCheckout) — handle `success=true|false`, poll `GET /payments/status/{appointmentId}` 3s×20
- [ ] `Paid` → `GET /appointments/my` shows `Confirmed`; `Failed` → retry same `appointmentId` with other `paymentMethod`
- [ ] `409 ReservationExpired` → clear slot, re-book
- [ ] FCM `PaymentConfirmation` + `AppointmentConfirmation` → auto-refresh
- [ ] Never log `client_secret`; redact `PaymobOrderId`
- [ ] Test both `wallet` (5690721 UIG → phone) and `card` (5671290 MIGS → pan) with `01000000000` (`ToPaymobFormat`)
- [ ] `GET /public/ads/active` anonymous, nullable `imageUrl`, full URL `{base}/files/{imageUrl}`, carousel "نشط" tag, tap→clinic, pull-to-refresh

### Web dashboard (clinic owner)
- [ ] `PUT /appointments/{id}/accept?paymentMethod=wallet|card&returnUrl=` generates `paymobRedirectUrl`; display / copy payment link
- [ ] Revenue page `GET /payments/appointments?status&method&page` shows `method` wallet/card; update badge `paymob_card` → بطاقة
- [ ] Ads: `GET /ads/packages` → `POST /clinics/{clinicId}/ads/orders {adPackageId,durationDays,logoImageUrl?,paymentMethod,returnUrl}` → redirect to `paymobRedirectUrl` → poll `GET /clinics/{clinicId}/ads` for `Active`

### Backend / DevOps
- [ ] `PaymobSettings` in `appsettings.*.json`: `IntegrationId=5671290`, `WalletIntegrationId=5690721`, `WebhookUrl=https://{host}/api/v1/payments/webhook`, `RedirectionUrl=https://{host}/api/v1/payments/result`, `HmacSecret`, `SecretKey`, `PublicKey`, `ApiKey`
- [ ] Verify HMAC `SHA512` and webhook reachability (`Paymob → webhook` test)
- [ ] Hangfire tables created via `IRelationalDatabaseCreator.CreateTablesAsync()` at startup, dashboard `/hangfire`
- [ ] Rate limiting `AspNetCoreRateLimit` in `appsettings.json`

---

## Future — Internal Wallet Ledger (not implemented)

Current `wallet` is Paymob external. For stored value (top-up once, spend per-booking without Paymob):

- Entities: `Wallet {UserId, Balance, Currency}`, `WalletTransaction {WalletId, Type:Credit/Debit, Amount, Reference, PaymentId?}`
- `POST /wallets/top-up {amount, paymentMethod: wallet|card}` → Paymob intention → webhook `MarkAsPaid` → `Balance+=amount`
- `POST /payments` with `paymentMethod:"internal_wallet"` → check `Balance>=total` → `Balance-=total` + `MarkAsManuallyPaid("internal_wallet")` + `Confirm` synchronously (no Paymob)
- Add `PaymentMethod.InternalWallet=3`, extend mapper, `ClinicHub.Domain/Entities/Wallet.cs`, `DbSet<Wallet>`, commands `TopUpWallet`/`DeductWallet`, row-level lock via `PaymentRefundGate` pattern.

Raise separate feature ticket if needed.

