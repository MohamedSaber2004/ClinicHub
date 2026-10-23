# Clinic Owner `IsCompleteProfile` — Mobile Integration README

> **Date:** 2026-09-23
> **Base URL:** `{host}/api/v{version}` → `v1` is `api/v1`. Docs at `/scalar/v1`.
> **Default culture:** Arabic (`Accept-Language: ar` / `en`).

This is the **single source of truth** for the mobile team on the new clinic-owner
onboarding flag: `GET /auth/profile` now returns `isCompleteProfile` for clinic
owners logging in from the **mobile app without a subscription**.

---

## Table of Contents

1. [What Changed](#1-what-changed)
2. [Flag Semantics](#2-flag-semantics)
3. [Mobile Flow (State Machine)](#3-mobile-flow-state-machine)
4. [Routes Summary](#4-routes-summary)
5. [Endpoint Contracts](#5-endpoint-contracts)
6. [Error Mapping](#6-error-mapping)
7. [cURL Examples](#7-curl-examples)
8. [Mobile Checklist](#8-mobile-checklist)

---

## 1. What Changed

| File | Change |
|---|---|
| `ClinicHub.Application/Features/Auth/DTOs/AuthDto.cs` | `UserProfileDto` gained `bool? IsCompleteProfile = null` (last positional param, optional → non-breaking) |
| `ClinicHub.Application/Features/Auth/Queries/GetUserProfile/GetUserProfileQueryHandler.cs` | Computes the flag **only** for `ClinicOwner` role (added `using ClinicHub.Domain.Enums`) |

No route, migration, or DI change. `GET api/v1/auth/profile` shape is unchanged
except one new nullable field inside `ApiResponse<UserProfileDto>.data`.

**Computation (server):**

```csharp
// Only when roles contains "ClinicOwner", otherwise null
clinicId = user.ClinicId
    ?? Clinic where ClinicAdminId == user.Id
    ?? Doctor.ClinicId where UserId == user.Id
    ?? JWT CurrentClinicId;

if (clinicId == null) → false
else {
  hasBookingConfig = Exists(BookingConfiguration where ClinicId && !IsDeleted)
  hasAvailability  = Exists(DoctorAvailability where ClinicId && !IsDeleted)
  isCompleteProfile = hasBookingConfig && hasAvailability
}
```

> "Clinic availability" = **≥ 1 non-deleted `DoctorAvailability` row for the owned
> clinic**. That is what makes the clinic actually bookable. There is no separate
> `ClinicAvailability` table.

---

## 2. Flag Semantics

| `roles` | `isCompleteProfile` | Meaning |
|---|---|---|
| `ClinicOwner` | `true` | Booking config **exists** AND ≥ 1 availability slot exists → go to home/subscription |
| `ClinicOwner` | `false` | Missing booking config, missing availability, or no owned clinic resolved → open Setup Wizard |
| anything else | `null` | Not applicable — **ignore the flag** (patients, staff, doctors, admins) |

Mobile rule:

```kotlin
if (profile.roles == "ClinicOwner" && profile.isCompleteProfile == false) {
    openSetupWizard()
} else {
    openHome()
}
```

> Never gate on the flag alone without checking `roles == "ClinicOwner"`.
> JSON casing is camelCase (`isCompleteProfile`).

---

## 3. Mobile Flow (State Machine)

```
POST /auth/login  (mobile — never blocks on subscription)
  → persist accessToken, refreshToken, clinicId, doctorId
GET /auth/profile
  → roles != ClinicOwner            → home (ignore flag, it is null)
  → ClinicOwner + true              → home / subscription screen
  → ClinicOwner + false             → SETUP WIZARD:
       A. GET booking-config → 200 (edit) / 404 (create)
          POST or PUT booking-config → saved
       B. GET doctors/me (resolve doctorId if login didn't return one)
          GET availability?doctorId&clinicId → show existing
          POST availability (≥ 1 slot, one call per day/window)
       C. GET /auth/profile again → expect true → close wizard
```

Why this works pre-subscription:

- Mobile login (`POST /auth/login`) does **not** check subscriptions.
  Only dashboard login (`POST /auth/login-web`) returns 403 without one — **don't use it on mobile**.
- `booking-config` and `availability` controllers have only `[RoleAuthorize]`,
  no `RequireSubscription` / `RequirePlanPermission` — they work with no plan.
- Do **not** use `GET/POST /admin/clinics/{id}/doctors` in the wizard — those
  require `ManageDoctors` plan permission and **will 403** without a subscription.

---

## 4. Routes Summary

Prefix `api/v{version:apiVersion}` → `api/v1`. Constants in `ClinicHub.API/Routes/ApiRoutes.cs`.

| # | Purpose | Method + Route | Auth |
|---|---|---|---|
| 1 | Login (mobile) | `POST /api/v1/auth/login` (`Auth.Login`) | anonymous |
| 2 | Get profile (+ new flag) | `GET /api/v1/auth/profile` (`Auth.Profile`) | `Bearer` any |
| 3 | Get booking config | `GET /api/v1/clinics/{clinicId}/booking-config` (`BookingConfig.GetByClinic`) | `Bearer` any |
| 4 | Create booking config | `POST /api/v1/clinics/{clinicId}/booking-config` (`BookingConfig.Create`) | `Bearer`, must be clinic admin |
| 5 | Update booking config | `PUT /api/v1/clinics/{clinicId}/booking-config` (`BookingConfig.Update`) | `Bearer`, must be clinic admin |
| 6 | My doctor profile (resolve `doctorId`) | `GET /api/v1/doctors/me` (`Doctors.GetMyProfile`) | `Bearer` |
| 7 | List availability | `GET /api/v1/availability?doctorId=&clinicId=` (`Availability.GetAllAvailability`) | `Bearer` |
| 8 | Create availability | `POST /api/v1/availability` (`Availability.Create`) | `Bearer` |
| 9 | Update availability | `PUT /api/v1/availability/{id}` (`Availability.Update`) | `Bearer` |
| 10 | Delete availability | `DELETE /api/v1/availability/{id}` (`Availability.Delete`) | `Bearer` |

Global headers: `Authorization: Bearer <jwt>`, `Content-Type: application/json`,
`Accept-Language: ar|en`. All success bodies are wrapped:

```json
{ "success": true, "data": { ... }, "message": "...", "statusCode": 200, "errors": {} }
```

---

## 5. Endpoint Contracts

### 5.1 `POST /api/v1/auth/login` — mobile login

**Request** (`LoginCommand`):

```json
{ "email": "owner@clinic.com", "password": "***", "fcmToken": null, "devicePlatform": null }
```

`devicePlatform` (`DevicePlatform` enum) is optional; send `fcmToken` + platform
to register push on login.

**Response `200` `data` (`AuthResponseDto`):**

```json
{
  "accessToken": "eyJ...", "refreshToken": "...",
  "fullName": "...", "email": "owner@clinic.com", "roles": "ClinicOwner",
  "id": "user-guid", "clinicId": "clinic-guid", "doctorId": "doctor-guid",
  "profilePictureUrl": null, "isFreelanceDoctor": false,
  "clinicStatus": "Active", "verificationStatus": "Approved",
  "isClinicSetupComplete": true
}
```

> **Cache `clinicId` + `doctorId` here** — `GetProfile` does not return them.

### 5.2 `GET /api/v1/auth/profile` — the flag

**Response `200` `data` (`UserProfileDto`):**

```json
{
  "id": "user-guid", "fullName": "...", "email": "owner@clinic.com",
  "gender": 1, "phoneNumber": "...", "birthDate": "1990-01-01",
  "profilePictureUrl": null, "language": 0,
  "roles": "ClinicOwner", "isFreelanceDoctor": false,
  "isCompleteProfile": false
}
```

- Owner without booking config → `false`.
- Owner without any availability → `false`.
- Owner with both → `true`.
- Non-owner → `null`.

### 5.3 `GET /api/v1/clinics/{clinicId}/booking-config`

**Response `200` `data` (`BookingConfigResponseDto`):**

```json
{
  "consultationFee": 300, "currency": "EGP",
  "maxAdvanceBookingDays": 30, "reservationTtlMinutes": 10,
  "cancellationWindowMinutes": 120
}
```

**Errors:** `404 BookingConfigNotFound` → go to create (§5.4).

### 5.4 `POST /api/v1/clinics/{clinicId}/booking-config` — create

**Request** (`CreateBookingConfigDto` as body, `clinicId` in route):

```json
{
  "consultationFee": 300,
  "maxAdvanceBookingDays": 30,
  "reservationTtlMinutes": 10,
  "cancellationWindowMinutes": 120
}
```

Rules: every number `> 0`. Currency is forced to `"EGP"` server-side.

**Response `201`:** same shape as §5.3.

**Errors:** `400 BookingConfigAlreadyExists` → use PUT (§5.5).
`403` → caller is not the clinic admin (`Clinic.ClinicAdminId != current user`).

### 5.5 `PUT /api/v1/clinics/{clinicId}/booking-config` — update

Same body/response as §5.4. **Errors:** `404 BookingConfigNotFound`
(nothing to update — POST instead).

### 5.6 `GET /api/v1/doctors/me` — resolve `doctorId`

Use when login did not return a `doctorId` (or to refresh it).
**Response `200` `data` (`DoctorDto`)** includes the doctor `id` and its
`clinicId`. **Errors:** `404 DoctorNotFound` → owner has no doctor record;
contact backend (normally created by `SetupClinic` / registration-approval).

### 5.7 `GET /api/v1/availability?doctorId=&clinicId=`

Both query params required. Returns the doctor's weekly slots
(`GetAvailableSlots` result: days with `{ slotId, startTime, endTime, isAvailable }`).
Empty list = second half of the flag is still `false`.

### 5.8 `POST /api/v1/availability` — create slot

**Request** (`CreateNewAvailabilityCommand`), one call per day/window:

```json
{
  "doctorId": "doctor-guid",
  "dayOfWeek": 1,
  "startTime": "09:00:00",
  "endTime": "17:00:00",
  "slotDurationMinutes": 30
}
```

- `dayOfWeek`: `0`=Sunday … `6`=Saturday.
- `startTime`/`endTime`: `"HH:mm:ss"`, `endTime > startTime`.
- `slotDurationMinutes`: `1`–`480`.
- Window must fit inside the clinic's `WorkingHoursStart/End` + `WorkingDays`
  (`ClinicScheduleGuard`), else `400 ClinicClosed`.
- Same doctor + same day overlapping window → `400 Overlap`.
- Doctor must already belong to a clinic, and to **your** clinic scope,
  else `400` / `403 ClinicNotFound`.

**Response `200` `data` (`AvailabilityDto`):**

```json
{
  "id": "slot-guid", "doctorId": "doctor-guid", "dayOfWeek": 1,
  "startTime": "09:00:00", "endTime": "17:00:00",
  "slotDurationMinutes": 30, "createdAt": "2026-09-23T10:00:00Z", "updatedAt": null
}
```

### 5.9 `PUT /api/v1/availability/{id}` / `DELETE /api/v1/availability/{id}`

- PUT body: same as §5.8 (route `id` wins). Response: updated `AvailabilityDto`.
- DELETE: no body. Response: `200` with success message.

---

## 6. Error Mapping

Wrapped in `ApiResponse<object>` (`ClinicHub.Application/Common/Models/ApiResponse.cs`)
via `ApiExceptionFilterAttribute`.

| HTTP | When | Mobile action |
|---|---|---|
| 401 | Missing/expired JWT, or wrong user | Re-login / refresh token |
| 403 | Booking POST/PUT by non-admin; availability outside own clinic scope | Use owner token + own `clinicId` |
| 404 `BookingConfigNotFound` | `GET` before first create, or `PUT` with nothing stored | `POST` instead |
| 400 `BookingConfigAlreadyExists` | `POST` twice | `PUT` instead |
| 400 validation | Fee/days/TTL/window `<= 0`; availability bad range/duration | Fix form inline |
| 400 `ClinicClosed` | Slot outside clinic working hours/days | Clamp to working hours |
| 400 `Overlap` | Same doctor+day overlapping window | Pick another window |
| 400 `DoctorNotFound` / doctor-must-have-clinic | `doctorId` wrong or unassigned | Re-fetch `GET /doctors/me` |
| 403 plan/permission | Only on `/admin/.../doctors` routes (not part of wizard) | Don't call them pre-subscription |

---

## 7. cURL Examples

```bash
base=https://api.clinichub.example
ownerJwt="Bearer <clinic_owner_jwt>"

# 1. Mobile login
curl -X POST "$base/api/v1/auth/login" -H "Content-Type: application/json" \
  -d '{"email":"owner@clinic.com","password":"***"}'
# → save accessToken, clinicId, doctorId

clinicId="clinic-guid-from-login"
doctorId="doctor-guid-from-login"

# 2. Profile + flag
curl "$base/api/v1/auth/profile" -H "Authorization: $ownerJwt"
# → data.isCompleteProfile: true|false|null

# 3a. Booking config — check
curl "$base/api/v1/clinics/$clinicId/booking-config" -H "Authorization: $ownerJwt"
# 404 → create:

# 3b. Booking config — create
curl -X POST "$base/api/v1/clinics/$clinicId/booking-config" \
  -H "Authorization: $ownerJwt" -H "Content-Type: application/json" \
  -d '{"consultationFee":300,"maxAdvanceBookingDays":30,"reservationTtlMinutes":10,"cancellationWindowMinutes":120}'

# 3c. Booking config — update (if POST said AlreadyExists)
curl -X PUT "$base/api/v1/clinics/$clinicId/booking-config" \
  -H "Authorization: $ownerJwt" -H "Content-Type: application/json" \
  -d '{"consultationFee":350,"maxAdvanceBookingDays":30,"reservationTtlMinutes":10,"cancellationWindowMinutes":120}'

# 4a. Resolve doctorId if needed
curl "$base/api/v1/doctors/me" -H "Authorization: $ownerJwt"

# 4b. List availability
curl "$base/api/v1/availability?doctorId=$doctorId&clinicId=$clinicId" -H "Authorization: $ownerJwt"

# 4c. Create availability (repeat per day)
curl -X POST "$base/api/v1/availability" \
  -H "Authorization: $ownerJwt" -H "Content-Type: application/json" \
  -d "{\"doctorId\":\"$doctorId\",\"dayOfWeek\":6,\"startTime\":\"09:00:00\",\"endTime\":\"17:00:00\",\"slotDurationMinutes\":30}"

# 5. Confirm
curl "$base/api/v1/auth/profile" -H "Authorization: $ownerJwt"
# → data.isCompleteProfile should now be true
```

---

## 8. Mobile Checklist

- [ ] Login via `POST /auth/login` (not `/auth/login-web`); persist tokens + `clinicId` + `doctorId`.
- [ ] After login **and** on app start, `GET /auth/profile`; branch on
      `roles == "ClinicOwner" && isCompleteProfile == false`.
- [ ] Wizard screen 1: `GET` booking-config → `POST` (404) or `PUT` (200); validate `> 0` client-side.
- [ ] Wizard screen 2: `GET /doctors/me` fallback → `POST /availability` until ≥ 1 slot; surface `Overlap` / `ClinicClosed` inline.
- [ ] Finish: re-`GET /auth/profile`, require `true` before leaving the wizard.
- [ ] Non-owners: never block on this flag (`null`).
- [ ] Keep `Accept-Language` header so Arabic/English errors match the app locale.
