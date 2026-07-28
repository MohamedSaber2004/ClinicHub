# Login Response — Flutter Integration Guide

## Overview

The login response now includes three new fields that tell the Flutter app exactly what state the user's account is in and what screen to navigate to next.

---

## Response Fields

| Field | Type | Possible Values | Description |
|-------|------|----------------|-------------|
| `clinicStatus` | `string?` | `null`, `"PendingApproval"`, `"Active"`, `"Suspended"` | Clinic status — only for ClinicOwner role |
| `verificationStatus` | `string?` | `null`, `"Pending"`, `"Approved"`, `"Rejected"` | Admin verification status — only for ClinicOwner |
| `isClinicSetupComplete` | `bool` | `true` / `false` | Whether clinic has completed its initial setup |

For non-ClinicOwner roles, all three fields are `null` / `false`.

---

## Full Login Response Example (ClinicOwner)

```json
{
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "7c4a...",
    "fullName": "Ahmed Ali",
    "email": "ahmed@clinic.com",
    "roles": "ClinicOwner",
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "clinicId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "profilePictureUrl": "https://...",
    "isFreelanceDoctor": false,
    "clinicStatus": "PendingApproval",
    "verificationStatus": "Approved",
    "isClinicSetupComplete": false
  },
  "message": "Ok",
  "statusCode": 200
}
```

---

## Complete Flow & State Machine

```
RegisterClinic
     │
     ▼
  [SignupResponse: IsPendingApproval = true]
     │  User cannot login yet
     ▼
Admin reviews → approves/rejects
     │
     ▼
Login
     │
     ├── Credentials wrong       → 401
     ├── Account inactive        → 403 "Account Pending Approval"
     ├── Pending verification    → 403 "Account Pending Approval"
     └── Success                 → 200 with status fields below
                                   (user can now log in)
```

---

## Flutter Navigation Decision

After successful login, use this decision map:

```dart
enum AppScreen {
  normalUserHome,
  clinicOwnerDashboard,
  clinicSetup,
  clinicPendingApproval,
  clinicRejected,
}

AppScreen resolveNextScreen(AuthResponseDto response) {
  // Not a clinic owner → normal user flow
  if (response.clinicStatus == null) {
    return AppScreen.normalUserHome;
  }

  // ClinicOwner cases
  switch (response.verificationStatus) {
    case 'Pending':
      return AppScreen.clinicPendingApproval;
    case 'Rejected':
      return AppScreen.clinicRejected;
  }

  // Approved — check clinic state
  switch (response.clinicStatus) {
    case 'PendingApproval':
      return AppScreen.clinicPendingApproval;
    case 'Suspended':
      // Show suspended message
      return AppScreen.clinicPendingApproval;
  }

  // Active — check if setup is needed
  if (!response.isClinicSetupComplete) {
    return AppScreen.clinicSetup;
  }

  return AppScreen.clinicOwnerDashboard;
}
```

### Screen mapping

| `clinicStatus` | `verificationStatus` | `isClinicSetupComplete` | Navigate to |
|---------------|---------------------|------------------------|-------------|
| `null` | `null` | `false` | Normal user home |
| `Active` | `Approved` | `true` | ClinicOwner dashboard |
| `Active` | `Approved` | `false` | **Setup clinic screen** |
| `PendingApproval` | `Approved` | `false` | Setup clinic screen |
| `PendingApproval` | `Pending` | `false` | Waiting approval screen |
| `PendingApproval` | `Rejected` | `false` | Rejection reason screen |
| `Suspended` | `Approved` | `false` | Contact support screen |

---

## Setup Clinic Screen

When `isClinicSetupComplete == false`, the Flutter app should navigate to a **setup clinic screen** that lets the owner fill in missing clinic details.

### What the setup screen should contain

```
┌──────────────────────────────────┐
│  Complete Clinic Setup          │
│                                  │
│  Clinic Name    [____________]   │
│  Address        [____________]   │
│  Phone          [____________]   │
│  Email          [____________]   │
│  Specialization [dropdown____]   │
│  Logo           [Upload Image]   │
│  Working Hours  [picker______]   │
│  Location       [Map Picker__]   │
│                                  │
│  [Save & Continue]               │
└──────────────────────────────────┘
```

### API call (POST `/api/v1/admin/clinics/setup`)

```http
POST /api/v1/admin/clinics/setup
Authorization: Bearer {accessToken}
Content-Type: application/json
```

```json
{
  "name": "ClinicHub Medical Center",
  "description": "Best cardiology clinic",
  "address": "123 Main Street, Cairo",
  "phone": "+201234567890",
  "email": "info@clinichub.com",
  "website": "https://clinichub.com",
  "logo": "https://cdn.clinichub.com/logos/123.jpg",
  "workingHours": "Sat-Thu 9:00-17:00",
  "workingHoursStart": "09:00",
  "workingHoursEnd": "17:00",
  "workingDays": ["Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday"],
  "specializationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "lat": 30.0444,
  "lng": 31.2357
}
```

### Setup endpoint response

```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "ClinicHub Medical Center",
    "address": "123 Main Street, Cairo",
    "phone": "+201234567890",
    "status": "Active"
  },
  "message": "Ok",
  "statusCode": 200
}
```

After successful setup, the user should be navigated to the **ClinicOwner dashboard**.

---

## How `isClinicSetupComplete` is determined

The `Clinic` entity has a dedicated `IsSetupComplete` boolean flag.

| Where | What happens to the flag |
|-------|-------------------------|
| `RegisterClinic` | `IsSetupComplete` = `false` (clinic needs admin approval first) |
| Admin approves | Flag stays `false` |
| `SetupClinic` endpoint | `IsSetupComplete` = `true` (clinic is fully set up) |
| `Login` | Reads `clinic.IsSetupComplete` directly |

The `SetupClinic` endpoint is the **only** place that sets this flag to `true`. So the Flutter app can rely on it completely — no field-based heuristics.

---

## Deep Linking Alternative

Instead of checking fields after login, you can use a **deep link** pattern:

```dart
// After login, the app receives a deep link action
// Example: "clinichub://setup-clinic?clinicId=xxx"

String? deepLinkAction = response.deepLink; // future enhancement
```

This is useful if you want to send users to specific screens via push notifications or external links.

---

## Enums Reference

### ClinicStatus

```dart
enum ClinicStatus {
  pendingApproval,
  active,
  suspended,
}
```

### VerificationStatus

```dart
enum VerificationStatus {
  pending,
  approved,
  rejected,
}
```

---

## Quick Checklist for Flutter Integration

1. [ ] Update `AuthResponseDto` model in Flutter to include `clinicStatus`, `verificationStatus`, `isClinicSetupComplete`
2. [ ] After login, call `resolveNextScreen()` to determine navigation destination
3. [ ] Build the **Setup Clinic screen** with the form fields listed above
4. [ ] Wire the Setup screen to `POST /api/v1/admin/clinics/setup`
5. [ ] On success, navigate to ClinicOwner dashboard
6. [ ] Add a **Waiting Approval screen** for when `verificationStatus == 'Pending'`
7. [ ] Add a **Rejected screen** for when `verificationStatus == 'Rejected'`
