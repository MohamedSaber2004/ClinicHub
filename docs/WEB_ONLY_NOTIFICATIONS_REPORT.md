# 🖥️ Web-Dashboard-Only Notification Types — Report (verified against implementation)

**Definition:** a type is **web-only** if every recipient is a dashboard role
(`SuperAdmin` / `ClinicOwner` / `Doctor` / `Staff`) and **no patient** ever receives it.
All rows below were **verified against the current sender code** (`SendToUserAsync` call sites),
not just the docs.

---

## 1. Web-only types (12) — implementation-verified

| # | Type | Arabic title | Recipients (as implemented) | Trigger | Verified sender code |
|---|------|-------------|------------------------------|---------|----------------------|
| 7 | `SubscriptionExpiring` | اشتراكك على وشك الانتهاء | First user of the clinic with `ClinicId` match (falls back to `ClinicAdminId`) | Daily job — subscription ends in 3/1 days | `Infrastructure/Services/BackgroundJobs/ExpiryReminderJob.cs` (`NotifyClinicOwnerAsync`) |
| 9 | `AdExpiring` | إعلانك على وشك الانتهاء | Same as #7 | Daily job — ad ends in 3/1 days | `ExpiryReminderJob.cs` |
| 10 | `AppointmentOutsideAvailability` | موعد خارج مواعيد الطبيب | Appointment's **doctor** (`Doctor.UserId`) + **clinic admin** (`ClinicAdminId`, or first clinic user when owner not set) | Hourly job — booking outside doctor availability | `DoctorAvailabilityValidationJob.cs` |
| 11 | `AppointmentOutsideWorkingHours` | موعد خارج ساعات عمل العيادة | Clinic admin (`ClinicAdminId`, or first clinic user when owner not set) | Hourly job — booking outside clinic hours | `ClinicWorkingHoursValidationJob.cs` |
| 12 | `NewBookingRequest` | حجز جديد | Doctor + clinic owner (`ClinicAdminId`) + all staff users with matching `ClinicId` | Patient creates an appointment | `Features/Appointments/Commands/CreateAppointment/CreateAppointmentCommandHandler.cs` |
| 13 | `ClinicRegistered` | تسجيل عيادة جديدة | All `SuperAdmin` users | Clinic registration submitted | `Features/Clinics/Commands/RegisterClinic/RegisterClinicCommandHandler.cs` |
| 14 | `ClinicApproved` | تمت الموافقة على العيادة | Clinic owner (`Clinic.ClinicAdminId`) | Superadmin approves the clinic | `Features/Admin/Commands/ApproveClinic/ApproveClinicCommandHandler.cs` |
| 15 | `ClinicRejected` | تم رفض العيادة | Clinic owner (`Clinic.ClinicAdminId`) | Superadmin rejects the clinic | `Features/Admin/Commands/RejectClinic/RejectClinicCommandHandler.cs` |
| 16 | `SupportTicketUpdate` | تحديث تذكرة الدعم | Ticket owner (`SupportTicket.UserId`) | Ticket status changed | `Features/Admin/Commands/UpdateSupportTicketStatus/UpdateSupportTicketStatusCommandHandler.cs` |
| 17 | `PaymentReceived` | تم استلام الدفع | Clinic owner (`Clinic.ClinicAdminId`) | Appointment payment confirmed (webhook) | `Features/Payment/Commands/ConfirmPaymentWebhook/ConfirmPaymentWebhookCommandHandler.cs` |
| 18 | `RevenueIncreased` | زيادة الإيرادات | All `SuperAdmin` users | Appointment payment confirmed (webhook) | `ConfirmPaymentWebhookCommandHandler.cs` |
| 19 | `AppointmentAccepted` | تم تأكيد الحجز | Appointment's **doctor** (`Doctor.UserId`) + **clinic owner** (`ClinicAdminId`) | Booking request accepted (any accept path) | `Application/Common/Services/AppointmentAcceptanceService.cs` |

## 2. Implementation nuances (differences from the doc wording)

1. **#7 / #9 (expiry reminders):** the code does **not** explicitly target the owner role.
   `NotifyClinicOwnerAsync` sends to the **first `ApplicationUser` found with `ClinicId == clinic.Id`**
   (any clinic user — could be staff), falling back to `Clinic.ClinicAdminId` only when no
   matching user exists. If you want the owner strictly, this is a candidate fix.
2. **#10 / #11 (validation jobs):** recipients come from `ResolveClinicAdminIdsAsync`, which uses
   `ClinicAdminId` when set, else the **first clinic user** — same owner-fallback pattern.
3. **#12 / #19:** deduplicated with `HashSet<Guid>` — if the doctor IS the clinic owner, only
   one notification is sent.
4. **#10 / #11:** job-level dedupe via `sentKeys` — a user gets the same type at most once per
   notification-window period.
5. **Superadmin sends (#13, #18):** all users with role `SuperAdmin`; both wrapped in
   try/catch so failures never break the business flow.

## 3. Shared with mobile patients (8)

`AppointmentReminder` (0), `NewMessage` (1), `PaymentConfirmation` (2),
`AppointmentConfirmation` (3), `AppointmentCancellation` (4), `SystemAnnouncement` (5),
`CancellationWindowClosed` (6), `RefundProcessed` (8)

> `NewMessage` (1) is the only type sent to **both** worlds (patients + dashboard roles in the
> same conversation).

## 4. Per-role breakdown (web only)

| Role | Web-only types |
|------|---------------|
| **SuperAdmin** | `ClinicRegistered` (13), `RevenueIncreased` (18) |
| **ClinicOwner** | `NewBookingRequest` (12), `ClinicApproved` (14), `ClinicRejected` (15), `PaymentReceived` (17), `AppointmentAccepted` (19), `AppointmentOutsideWorkingHours` (11), `AppointmentOutsideAvailability` (10), `SubscriptionExpiring` (7), `AdExpiring` (9), `SupportTicketUpdate` (16) |
| **Doctor** | `NewBookingRequest` (12), `AppointmentAccepted` (19), `AppointmentOutsideAvailability` (10) |
| **Staff** | `NewBookingRequest` (12) |

## 5. Why this matters

- **Frontend:** only these 12 types need role-based hub navigation on the dashboard
  (`fcm.js` `APPOINTMENT_TYPES` / `CLINIC_TYPES` / `SUPPORT_TYPES` groups); the rest are
  patient-facing and land on the notifications hub.
- **Mobile:** these 12 types should **not** appear in the patient app's push handling —
  patients are never recipients; mobile only handles §3 types.
- **Backend:** all web-only types are append-only enum values (`>= 7`) — no DB migration.

Reference: full payloads in `docs/NOTIFICATION_TYPES_README.md`,
dashboard scenarios in `docs/WEB_DASHBOARD_NOTIFICATIONS_README.md`.
