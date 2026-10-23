# Free Onboarding: Booking-Config + Availability for Clinic Dashboard — Design Spec

**Date:** 2026-09-18
**Status:** Approved (all 3 design sections approved by stakeholder)
**Plan:** `docs/plans/2026-09-18-booking-availability-onboarding.md`

## 1. Goal

A newly approved clinic owner (no subscription) can fully set up their mobile
dashboard: booking configuration (add/update/get) and per-doctor availability
(add/update/get/delete). Paid features stay subscription-gated.

## 2. Decisions (locked)

1. **Free onboarding endpoints** — no trial record, no expiry. Booking-config,
   availability, and basic dashboard stats are free forever.
2. **Availability is per-doctor** via existing `/availability` endpoints. No new
   endpoints, no clinic-level template.
3. **Booking-config keeps `/{clinicId}` routes** — no `/my` variants.

## 3. Endpoint contracts (Approach A)

### 3.1 Booking configuration — ` /api/v{ver}/clinics/{clinicId}/booking-config`

| Op | Verb | Auth | Change |
|----|------|------|--------|
| Get | GET | `RoleAuthorize` (open; patients need the fee) | none |
| Add | POST | `RoleAuthorize` | REMOVE `RequirePlanPermission(OnlineBooking)` |
| Update | PUT | `RoleAuthorize` | REMOVE `RequirePlanPermission(OnlineBooking)` |

Request body (both POST and PUT, currency is server-set):
```json
{
  "consultationFee": 300,
  "maxAdvanceBookingDays": 30,
  "reservationTtlMinutes": 10,
  "cancellationWindowMinutes": 120
}
```

Response (`BookingConfigResponseDto`, 200 on PUT, 201 on POST):
```json
{
  "consultationFee": 300,
  "currency": "EGP",
  "maxAdvanceBookingDays": 30,
  "reservationTtlMinutes": 10,
  "cancellationWindowMinutes": 120
}
```

Ownership: `clinic.ClinicAdminId == caller` else `403 Forbidden` inside the
standard `ApiResponse` envelope. Cross-clinic access is always `403`, never
`404` (no id-oracle). POST when a config already exists returns `400` with a
message telling mobile to `PUT` instead.

Validation: `consultationFee >= 0`, `maxAdvanceBookingDays > 0`,
`reservationTtlMinutes > 0`, `cancellationWindowMinutes > 0`.

### 3.2 Availability (per doctor) — `/api/v{ver}/availability...`

| Op | Verb | Auth | Change |
|----|------|------|--------|
| Get week | `GET /availability?doctorId={d}&clinicId={c}` | `RoleAuthorize` | none (already free) |
| Add | `POST /availability` | `RoleAuthorize` | ADD ownership guard |
| Update | `PUT /availability/{id}` | `RoleAuthorize` | ADD ownership guard + null-safety |
| Delete | `DELETE /availability/{id}` | `RoleAuthorize` | ADD ownership guard |

POST body (`CreateNewAvailabilityCommand`):
```json
{
  "doctorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "dayOfWeek": 6,
  "startTime": "09:00:00",
  "endTime": "14:00:00",
  "slotDurationMinutes": 30
}
```
`dayOfWeek`: `0 = Sunday … 6 = Saturday` (System.Text.Json serializes the
`DayOfWeek` enum as a number by default; mobile sends numbers).

Response (`AvailabilityDto`):
```json
{
  "id": "guid",
  "doctorId": "guid",
  "dayOfWeek": 6,
  "startTime": "09:00:00",
  "endTime": "14:00:00",
  "slotDurationMinutes": 30,
  "createdAt": "2026-09-18T10:00:00",
  "updatedAt": null
}
```

Ownership guard (all writes): the target doctor's `ClinicId` must equal the
caller's resolved clinic scope (token `ClinicId` claim, else
`ClinicScopeResolver` DB fallback) else `403`. Same rule resolves which
records Update/Delete may touch (prevents cross-clinic writes AND stale-token
failures).

Overlap rule: a new/updated window must satisfy `startTime < endTime`,
`slotDurationMinutes > 0`, and must not overlap another non-deleted window
for the same doctor + day. Violation returns `400` naming the conflict.

## 4. Mobile dashboard workflow

1. Owner logs in → `AuthResponseDto` (`clinicId`, `isClinicSetupComplete`).
2. If setup incomplete → Setup modal → `POST /admin/clinics/setup`
   (updates the owner's own clinic in place).
3. Dashboard loads booking-config: `GET clinics/{id}/booking-config`.
   `404` → show form → `POST`. Later edits → `PUT`.
4. Availability tab: pick doctor → `GET /availability?doctorId&clinicId` →
   render 7-day grid → `POST` / `PUT` / `DELETE` windows.
5. After Setup, mobile calls the refresh-token endpoint so the access token
   carries the `ClinicId` claim.
6. Paid screens (advanced reports, staff/doctors management, booking intake)
   keep their gates: on `403`, mobile shows the Subscribe screen.

## 5. Business rules

- Booking-config + availability + basic stats are free forever for active
  clinics. No trial row, no expiry job, no Paymob involvement.
- One booking-config row per clinic (POST = first time, PUT = after).
- Availability windows: `start < end`, `slot > 0`, no same-doctor/day overlap.
- Money: `consultationFee >= 0`; currency is always `EGP`, set server-side.
- Cross-clinic reads/writes are `403`, never `404`.
- Soft-delete everywhere (`IsDeleted`); nothing is hard-deleted.

## 6. Out of scope (YAGNI)

- Trial subscriptions, free-tier permission enum, `/my` route variants,
  clinic-level weekly templates, recurring availability exceptions/holidays,
  changing paid gates on other endpoints.

## 7. Files the plan will touch

- `ClinicHub.API/Controllers/Version1/BookingConfigurationsController.cs`
  (remove 2 gate attributes)
- `ClinicHub.Application/Features/Booking/BookingConfig/Commands/CreateBookingConfig/CreateBookingConfigCommandHandler.cs`
  (403 shape + duplicate-config 400)
- `ClinicHub.Application/Features/Availability/Commands/CreateNewAvailability/CreateNewAvailabilityCommandHandler.cs`
  (ownership guard + overlap check)
- `ClinicHub.Application/Features/Availability/Commands/UpdateExistingAvailability/UpdateExistingAvailabilityCommandHandler.cs`
  (ownership guard + null-safety + overlap check)
- `ClinicHub.Application/Features/Availability/Commands/DeleteAvailability/DeleteAvailabilityCommandHandler.cs`
  (ownership guard)
- `ClinicHub.Application/Features/Availability/Commands/CreateNewAvailability/CreateNewAvailabilityCommandValidator.cs`,
  `.../UpdateExistingAvailability/UpdateExistingAvailabilityCommandValidator.cs`
  (overlap + range rules)
- No new files. No migrations. No DI changes.
