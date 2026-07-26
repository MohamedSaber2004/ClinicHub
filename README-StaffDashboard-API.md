# Staff Dashboard — API Integration Guide

Base URL: `/api/v{version}` (current: `v1`)

All responses are wrapped in `ApiResponse<T>`:

```json
{
  "success": true,
  "data": { ... },
  "message": "...",
  "errors": {}
}
```

All endpoints require `Authorization: Bearer {token}` header with a **Staff** user role.

---

## Patient Journey — Status Flow

```
Appointment: pending  ──[Approve]──▶  confirmed  ──[Check-in]──▶  confirmed  ──[Complete]──▶  completed
Queue:       —                        — (not in queue)            waiting                        completed
```

| Step | Backend Status | Appointments View | Queue View |
|------|---------------|-------------------|------------|
| Patient books | `Pending` | pending (قيد الانتظار) | — |
| Staff approves | `Accepted` | confirmed (مؤكد) | — (not checked in yet) |
| Patient checks in | `Confirmed` | confirmed (مؤكد) | waiting (في الانتظار) |
| Doctor completes | `Completed` | completed (منتهي) | completed (مكتمل) |

---

## Paginated Endpoints — Common Contract

Two endpoints return paginated data using query parameters. The response shape is identical:

### Request Query Parameters

| Param | Type | Default | Max | Description |
|-------|------|---------|-----|-------------|
| `pageNumber` | int | `1` | — | Page index (1-based) |
| `pageSize` | int | `10` (Appts) / `10` (Queue) | `100` | Items per page |

### Response Shape

```json
{
  "success": true,
  "data": {
    "items": [ ... ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 3,
    "totalCount": 22,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### Field Reference

| Field | Type | Notes |
|-------|------|-------|
| `items` | array | Page of items |
| `pageNumber` | int | Current page (1-based) |
| `pageSize` | int | Items per page |
| `totalPages` | int | Total pages available |
| `totalCount` | int | Total items across all pages |
| `hasPreviousPage` | bool | `false` when on page 1 |
| `hasNextPage` | bool | `false` when on last page |

### Frontend Rendering Rules

- `totalPages <= 0` or `totalCount === 0` → hide pagination entirely, show "no data" state
- `totalPages === 1` → show static page indicator "1" (not clickable)
- `totalPages > 1` → show clickable page links (`« 1 2 3 … »`)
- `hasPreviousPage` → enable/disable "Previous" button
- `hasNextPage` → enable/disable "Next" button

---

## 1. Dashboard Stats

**`GET /api/v1/staff/dashboard/stats`** *(no pagination)*

### Response `200 OK`

```json
{
  "success": true,
  "data": {
    "totalAppointments": 24,
    "checkedIn": 18,
    "waiting": 6,
    "completed": 8
  }
}
```

| Field | Meaning |
|-------|---------|
| `totalAppointments` | All appointments today |
| `checkedIn` | Confirmed / Accepted (checked-in) |
| `waiting` | Pending / Reserved (not yet checked) |
| `completed` | Completed |

---

## 2. Appointments List — **Paginated**

**`GET /api/v1/staff/appointments?pageNumber=1&pageSize=10&status=&date=&patientName=`**

### Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| `pageNumber` | int | No | Default `1` |
| `pageSize` | int | No | Default `10`, max `100` |
| `status` | int | No | `0=Pending, 1=Confirmed, 2=Cancelled, 3=Completed, 4=Reserved, 5=NoShow, 6=Accepted, 7=Rejected` |
| `date` | string `yyyy-MM-dd` | No | Defaults to today |
| `patientName` | string | No | Partial match (patient name or booker name) |

### Response `200 OK`

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "patient": {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "name": "محمد عمر",
          "initial": "م"
        },
        "doctor": {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "name": "د. سارة أحمد",
          "specialty": "أمراض القلب"
        },
        "specialty": "أمراض القلب",
        "date": "2026-07-26",
        "time": "09:00",
        "status": "pending",
        "statusLabel": "قيد الانتظار",
        "statusClass": "badge-warning",
        "phone": "0501112222"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 3,
    "totalCount": 22,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### Status → Frontend Mapping

| `status` value | `statusLabel` | `statusClass` | Action Available |
|---|---|---|---|
| `"pending"` | قيد الانتظار | `badge-warning` | Approve / Reject |
| `"confirmed"` | مؤكد | `badge-success` | Check-in |
| `"cancelled"` | ملغي | `badge-danger` | — (read-only) |
| `"completed"` | منتهي | `badge-info` | — (read-only) |

### Filtering by Status

The `status` filter accepts an **integer** matching the `AppointmentStatus` enum:

```
Pending = 0, Confirmed = 1, Cancelled = 2, Completed = 3,
Reserved = 4, NoShow = 5, Accepted = 6, Rejected = 7
```

Example: `?status=0` returns only pending appointments.

---

## 3. Approve Appointment

**`PUT /api/v1/staff/appointments/{id}/approve`** *(no pagination)*

### Path Parameter

| Param | Type | Description |
|-------|------|-------------|
| `id` | GUID | Appointment ID |

### Response `200 OK`

```json
{
  "success": true,
  "data": true,
  "message": "Appointments.Accepted"
}
```

### Errors

- `400` — Clinic context missing, not authorized, or appointment not in a respondable state
- `404` — Appointment not found

---

## 4. Reject Appointment

**`PUT /api/v1/staff/appointments/{id}/reject`** *(no pagination)*

### Path Parameter

| Param | Type | Description |
|-------|------|-------------|
| `id` | GUID | Appointment ID |

### Request Body

```json
{
  "reason": "الموعد غير مناسب"
}
```

| Field | Type | Required |
|-------|------|----------|
| `reason` | string | No (max 500 chars) |

### Response `200 OK`

```json
{
  "success": true,
  "data": true,
  "message": "Appointments.Rejected"
}
```

---

## 5. Check-In Patient

**`PUT /api/v1/staff/appointments/{id}/check-in`** *(no pagination)*

Transitions the appointment from `Accepted`/`Reserved` → `Confirmed`.

### Response `200 OK`

```json
{
  "success": true,
  "data": true,
  "message": "Appointments.CheckedIn"
}
```

---

## 6. Complete Appointment

**`PUT /api/v1/staff/appointments/{id}/complete`** *(no pagination)*

Transitions the appointment from `Accepted`/`Confirmed` → `Completed`.

### Response `200 OK`

```json
{
  "success": true,
  "data": true,
  "message": "Appointments.Completed"
}
```

---

## 7. Queue — **Paginated**

**`GET /api/v1/staff/queue?pageNumber=1&pageSize=10`**

### Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| `pageNumber` | int | No | Default `1` |
| `pageSize` | int | No | Default `10`, max `100` |

### Response `200 OK`

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "queueNumber": 1,
        "patient": {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "name": "محمد عمر",
          "initial": "م"
        },
        "doctor": {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "name": "د. سارة أحمد",
          "specialty": "أمراض القلب"
        },
        "time": "09:00",
        "status": "in-progress",
        "statusLabel": "قيد الكشف",
        "statusClass": "badge-primary"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 2,
    "totalCount": 12,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

**Note:** `queueNumber` is sequential across the **entire** queue (not per page).  
Example: Page 2 will have `queueNumber: 11, 12, 13, …`

### Queue Status Mapping

| `status` | `statusLabel` | `statusClass` | Meaning | Action |
|---|---|---|---|---|---|
| `"waiting"` | في الانتظار | `badge-warning` | Checked in, waiting for doctor | Complete |
| `"completed"` | مكتمل | `badge-success` | Done | — (read-only) |

> **Note:** The queue shows only patients who have physically **checked into the clinic** (`Confirmed` status).  
> Approved appointments (`Accepted`) do not appear here until the patient checks in.  
> The `"in-progress"` state is not yet represented in the queue (requires a future DB migration to add an `InProgress` appointment status).

---

## 8. Register Walk-In Patient

**`POST /api/v1/staff/patients/register`** *(no pagination)*

### Request Body

```json
{
  "fullName": "أحمد محمد",
  "phoneNumber": "0501234567",
  "email": null,
  "age": 30,
  "gender": 1,
  "doctorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "clinicId": "00000000-0000-0000-0000-000000000000",
  "appointmentDate": "2026-07-26T00:00:00",
  "startTime": "09:00",
  "endTime": "09:30",
  "appointmentType": 0,
  "complaint": "ألم في الأسنان",
  "chronicDiseases": null
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `fullName` | string | ✅ | |
| `phoneNumber` | string | ✅ | |
| `email` | string | ❌ | |
| `age` | int | ❌ | |
| `gender` | int | ❌ | `1=Male, 2=Female` |
| `doctorId` | GUID | ✅ | |
| `clinicId` | GUID | ✅ | Send `00000000-0000-0000-0000-000000000000` to auto-use staff clinic |
| `appointmentDate` | ISO date `yyyy-MM-dd` | ✅ | |
| `startTime` | string `HH:mm` | ✅ | |
| `endTime` | string `HH:mm` | ✅ | |
| `appointmentType` | int | ✅ | `0=Examination, 1=FollowUp` |
| `complaint` | string | ✅ | |
| `chronicDiseases` | string | ❌ | |

### Response `201 Created`

```json
{
  "success": true,
  "data": {
    "appointmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "patientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "queueNumber": 7,
    "message": "تم تسجيل المريض بنجاح"
  }
}
```

---

## 9. Doctor List (Dropdown)

**`GET /api/v1/staff/doctors`** *(no pagination)*

### Response `200 OK`

```json
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "د. سارة أحمد",
      "specialty": "أمراض القلب"
    },
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "د. عمار السيد",
      "specialty": "جلدية"
    }
  ]
}
```

Returns all doctors in the staff user's clinic.

---

## 10. Doctor Schedule

**`GET /api/v1/staff/doctors/{doctorId}/schedule?date=2026-07-26`** *(no pagination)*

### Path & Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| `doctorId` | GUID | ✅ (path) | |
| `date` | string `yyyy-MM-dd` | ❌ (query) | Defaults to today |

### Response `200 OK`

```json
{
  "success": true,
  "data": {
    "doctor": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "د. سارة أحمد",
      "specialty": "أمراض القلب"
    },
    "date": "2026-07-26",
    "appointments": [
      {
        "patient": {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "name": "محمد عمر",
          "initial": "م"
        },
        "time": "09:00",
        "statusLabel": "مؤكد",
        "statusClass": "badge-success"
      }
    ]
  }
}
```

---

## Error Response Shape

All errors return `success: false`:

```json
{
  "success": false,
  "message": "Appointment not found",
  "errors": {
    "AppointmentId": ["Appointment not found"]
  },
  "statusCode": 404
}
```

### Common HTTP Status Codes

| Code | Meaning |
|------|---------|
| `200` | Success |
| `201` | Created (walk-in registration) |
| `400` | Bad request — validation error or business rule violation. Check `errors` for field-level details |
| `401` | Unauthorized — missing or invalid token |
| `403` | Forbidden — wrong user role or missing plan permission |
| `404` | Resource not found |

---

## Quick Reference — All Endpoints

| # | Method | Route | Paginated | Query Params |
|--|--------|-------|-----------|-------------|
| 1 | `GET` | `/staff/dashboard/stats` | ❌ | — |
| 2 | `GET` | `/staff/appointments` | ✅ | `pageNumber`, `pageSize`, `status`, `date`, `patientName` |
| 3 | `PUT` | `/staff/appointments/{id}/approve` | ❌ | — |
| 4 | `PUT` | `/staff/appointments/{id}/reject` | ❌ | — (body: `reason`) |
| 5 | `PUT` | `/staff/appointments/{id}/check-in` | ❌ | — |
| 6 | `PUT` | `/staff/appointments/{id}/complete` | ❌ | — |
| 7 | `GET` | `/staff/queue` | ✅ | `pageNumber`, `pageSize` |
| 8 | `POST` | `/staff/patients/register` | ❌ | — (body) |
| 9 | `GET` | `/staff/doctors` | ❌ | — |
| 10 | `GET` | `/staff/doctors/{doctorId}/schedule` | ❌ | `date` |

---

## Root Cause Analysis — Status Not Showing on Appointments Page

This section documents integration failures reported from the frontend. Use it to reproduce and fix the issue.

### Symptom

The appointments page loads but the **status badge column is empty** — no label text, no background color, or the entire column is blank. In some cases the page shows "no data" despite appointments existing in the database.

### Full Request/Response Chain

```
Frontend (Razor/Flutter/SPA)
  ↓  GET /Staff/Appointments?pageNumber=1&pageSize=10&status=&date=&patientName=
StaffController (MVC Gateway — if present)
  ↓  HTTP GET
Backend API — StaffController.GetAppointments()  [StaffController.cs:41]
  ↓  Ok(result) wraps in ApiResponse<PagginatedResult<StaffAppointmentDto>>  [BaseApiController.cs:20]
  ↓  System.Text.Json serialization (default camelCase, no custom converters)
JSON response
  ↓  Frontend deserializes & renders
```

### Code-Level Root Causes (Verified by Inspection)

#### 1. `PagginatedResult.Items` is `IReadOnlyCollection<T>` → risk of object instead of array

**File:** `ClinicHub.Application/Common/Models/PagginatedResult.cs:10`

```csharp
public IReadOnlyCollection<T> Items { get; }  // read-only property, no setter
```

**Problem:** `System.Text.Json` serializes `IReadOnlyCollection<T>` based on its **runtime type** (`List<T>`). Under standard settings this produces a JSON array `[...]`. However, if the runtime type is a custom wrapper without proper `IEnumerable` support, or if an intermediary deserializer (MVC gateway, BFF) uses a constructor-based approach that fails, `items` becomes a JSON object `{ "0": {...}, "1": {...} }` instead of `[ {...}, {...} ]`.

**Diagnosis:** Hit the endpoint directly with a tool (Postman, cURL) and inspect `data.items` in the raw response.

```powershell
curl -H "Authorization: Bearer $(token)" "http://localhost:5000/api/v1/staff/appointments?pageNumber=1&pageSize=10"
```

Expected: `"items": [ {...}, {...} ]` — an **array** (`[...]`).  
If you see `"items": { "0": {...}, "1": {...} }` — an **object** (`{...}`), that's the bug.

**Fix:** Either remove the intermediary deserialization, or add a custom `JsonConverter<T>` that forces array serialization for `IReadOnlyCollection<T>`.

---

#### 2. `CurrentClinicId` is null → empty result returned

**File:**  
- `ClinicHub.API/Services/CurrentUserService.cs:16,43-47`  
- `ClinicHub.Application/Features/StaffDashboard/Queries/GetStaffAppointments/GetStaffAppointmentsQueryHandler.cs:23-25`

```csharp
// CurrentUserService.cs
public Guid? CurrentClinicId { get; }      // null when no "ClinicId" claim in JWT

var clinicIdClaim = httpContext?.User?.FindFirst("ClinicId")?.Value;
if (Guid.TryParse(clinicIdClaim, out var clinicId))
    CurrentClinicId = clinicId;

// GetStaffAppointmentsQueryHandler.cs
if (clinicId == null)
    return new PagginatedResult<StaffAppointmentDto>(Array.Empty<StaffAppointmentDto>(), 0);
```

**Problem:** When the JWT token lacks a valid `ClinicId` claim, `CurrentClinicId` is null. The handler returns a **valid `200 OK`** with `data.items: []` (empty array) and `totalCount: 0`. The frontend sees a successful response with no data, shows "no appointments", and the status column never renders.

**Note:** This case returns `pageSize: 20` (from `PagginatedResult.DefaultPageSize`) even if the frontend requested `pageSize: 10`, due to the empty constructor call not passing the request's page size. This can confuse the paginator.

**Diagnosis:** Check the response for:
```json
{ "success": true, "data": { "items": [], "totalCount": 0, "pageSize": 20 } }
```

vs expected:
```json
{ "success": true, "data": { "items": [], "totalCount": 0, "pageSize": 10 } }
```

**Fix:**  
- Ensure the JWT token includes a valid `ClinicId` claim (GUID).  
- Fix the empty-result constructor to preserve the requested page size:
  ```csharp
  return new PagginatedResult<StaffAppointmentDto>(Array.Empty<StaffAppointmentDto>(), 0, request.PageNumber, request.PageSize);
  ```

---

#### 3. `StaffAppointmentDto` fields are correct — but only if camelCase JSON

**File:** `ClinicHub.Application/Features/StaffDashboard/DTOs/StaffAppointmentDto.cs:11-13`

```csharp
public string Status { get; set; } = null!;       // → "status"
public string StatusLabel { get; set; } = null!;  // → "statusLabel"
public string StatusClass { get; set; } = null!;  // → "statusClass"
```

These are `string` type (not enum), set via `StaffDashboardStatusHelper` which always returns a valid string. When serialized with default `System.Text.Json` (camelCase policy), they produce the correct JSON field names.

**Problem if the response uses PascalCase:** If an intermediary service (MVC gateway, proxy, logging middleware) re-serializes with different settings, `Status` becomes `"Status"` (uppercase S) and the frontend gets `undefined`.

**Diagnosis:** Inspect the raw JSON response for field casing:
- ✅ Correct: `"status": "pending", "statusLabel": "قيد الانتظار", "statusClass": "badge-warning"`
- ❌ Wrong: `"Status": "pending", "StatusLabel": "قيد الانتظار", "StatusClass": "badge-warning"`

**Fix:** Ensure all intermediary layers preserve camelCase. The backend API is already correct — check if a BFF or MVC controller re-serializes with a different `JsonSerializerOptions`.

---

#### 4. AppointmentDate exact-match filter can miss data

**File:** `ClinicHub.Application/Features/StaffDashboard/Queries/GetStaffAppointments/GetStaffAppointmentsQueryHandler.cs:39-40`

```csharp
var targetDate = request.Date?.Date ?? DateTime.Today;
query = query.Where(a => a.AppointmentDate == targetDate);
```

**Problem:** The `==` comparison requires an exact match. If the stored `AppointmentDate` has any time component beyond midnight (e.g., `2026-07-26T00:00:01`), it won't match `DateTime.Today` (`2026-07-26T00:00:00`).  

The `Appointment` entity constructor strips the time via `.Date`, so in-memory objects are correct. But if the DB value was set from a different code path that didn't strip the time, the filter silently excludes it.

Also, if the frontend sends a `Date` parameter with a non-midnight time (`?date=2026-07-26T09:00:00`), `request.Date?.Date` converts it to midnight — so this works. But if the DB value is `2026-07-27T00:00:00` (next day), `DateTime.Today` on the server might differ from the client's timezone.

**Diagnosis:** Check the SQL query EF Core generates:
```
WHERE a.AppointmentDate = '2026-07-26T00:00:00'
```
If the DB values are `2026-07-26T00:00:01` or similar, the query returns zero rows.

**Fix:** Use a range comparison instead of equality:
```csharp
var targetDate = request.Date?.Date ?? DateTime.Today;
query = query.Where(a => a.AppointmentDate >= targetDate && a.AppointmentDate < targetDate.AddDays(1));
```

Or change `AppointmentDate` to `DateOnly` in the entity and database, which eliminates the time component entirely.

---

### Summary of Diagnostics to Run

| # | Check | How | Expected |
|---|-------|-----|----------|
| 1 | Raw JSON of `GET /api/v1/staff/appointments` | cURL / Postman | `items` is array `[]`, fields are camelCase, `statusLabel`/`statusClass` are present |
| 2 | JWT token contains `ClinicId` claim | Decode token (jwt.io) | Claim `ClinicId` with valid GUID |
| 3 | `AppointmentDate` in DB | SQL query | Stored as midnight `2026-07-26T00:00:00` without time drift |
| 4 | Intermediary service re-serialization | Check MVC gateway code | No `PascalCase` re-serialization between API and frontend |
| 5 | Empty-result `pageSize` mismatch | cURL with clinicId=null | `pageSize` matches what frontend sent, not default 20 |
