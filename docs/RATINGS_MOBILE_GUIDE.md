# 📱 Ratings — Mobile App Integration Guide (Patient App)

> What the **patient mobile app** needs to do to let patients rate their visit. The backend is **fully implemented** — the mobile team only needs to **consume** the endpoints below. All submission happens in the patient app; viewing/managing happens on the web dashboards (read-only).

---

## 🎯 What the Mobile App Must Do

After a patient completes a visit, show a **rating sheet with 3 sections**:

| Section | `type` | Target field | Example |
|---|---|---|---|
| 1. تقييم الطبيب (Doctor rating) | `1` | `doctorId` | `"doctorId": "..."` |
| 2. تقييم العيادة (Clinic rating) | `2` | `clinicId` | `"clinicId": "..."` |
| 3. نظافة المكان (Place cleanliness) | `3` | `clinicId` | `"clinicId": "..."` |

Each section: **stars (1–5)**. At the end of the sheet, the patient writes **ONE general review text** (optional, max 1000 chars) — it is **shared**: the backend stores the same review text on all three rating rows, so it appears under every section on the web dashboard.

> ⚠️ **`type` is an integer, not a string** — the backend uses default JSON enum serialization (`1` / `2` / `3`), there is **no** `JsonStringEnumConverter`. Sending `"type": "Clinic"` fails with a 400.

---

## 📡 Endpoints

| Method | Endpoint | Auth | Purpose |
|---|---|---|---|
| `POST` | `{base}/api/v1/ratings/batch` | ✅ Bearer (any authenticated role) | **Submit all sections + ONE general review in a single call** (recommended for new builds) |
| `POST` | `{base}/api/v1/ratings` | ✅ Bearer | Submit one rating row (legacy, per-section) |
| `GET` | `{base}/api/v1/doctors/{doctorId}/ratings` | ✅ Bearer | List doctor ratings (type=1) |
| `GET` | `{base}/api/v1/clinics/{clinicId}/ratings` | ✅ Bearer | List clinic ratings (type=2) |
| `GET` | `{base}/api/v1/clinics/{clinicId}/place-cleanliness-ratings` | ✅ Bearer | List cleanliness ratings (type=3) |

> The GET endpoints are mainly used by the web dashboard. The patient app only **needs** `POST /ratings/batch`; the doctor/clinic averages are already returned by the existing mobile doctor-details endpoint (`GET /doctors/{doctorId}/details` → `averageRating`).

---

## 🧾 Submit All Ratings + General Review — `POST {base}/api/v1/ratings/batch`

Creates **all three rating rows atomically** (one DB transaction) with the same `review` text. If any section was already rated by the user → **400** and nothing is saved.

**Request:**
```json
{
  "doctorId": "3f9a0000-0000-0000-0000-000000000001",
  "clinicId": null,
  "doctorValue": 5,
  "clinicValue": 4,
  "cleanlinessValue": 4,
  "review": "تجربة ممتازة عموماً، دكتور راقي والعيادة نظيفة"
}
```

> `doctorId` is **optional**: if omitted, the doctor section is skipped and only clinic + cleanliness rows are created (`clinicId` then becomes required).

**Response (`ApiResponse<List<RatingDto>>`, HTTP 201):**
```json
{
  "success": true,
  "message": "تم إرسال التقييم بنجاح",
  "data": [
    {
      "id": "7d1c0000-0000-0000-0000-000000000009",
      "type": 1,
      "userId": "2f9a0000-0000-0000-0000-000000000004",
      "userName": "محمد أحمد",
      "doctorId": "3f9a0000-0000-0000-0000-000000000001",
      "clinicId": null,
      "value": 5,
      "review": "تجربة ممتازة عموماً، دكتور راقي والعيادة نظيفة",
      "createdAt": "2026-08-09T21:10:00"
    },
    {
      "id": "7d1c0000-0000-0000-0000-00000000000a",
      "type": 2,
      "userId": "2f9a0000-0000-0000-0000-000000000004",
      "userName": "محمد أحمد",
      "doctorId": null,
      "clinicId": "4a1c0000-0000-0000-0000-000000000002",
      "value": 4,
      "review": "تجربة ممتازة عموماً، دكتور راقي والعيادة نظيفة",
      "createdAt": "2026-08-09T21:10:00"
    },
    {
      "id": "7d1c0000-0000-0000-0000-00000000000b",
      "type": 3,
      "userId": "2f9a0000-0000-0000-0000-000000000004",
      "userName": "محمد أحمد",
      "doctorId": null,
      "clinicId": "4a1c0000-0000-0000-0000-000000000002",
      "value": 4,
      "review": "تجربة ممتازة عموماً، دكتور راقي والعيادة نظيفة",
      "createdAt": "2026-08-09T21:10:00"
    }
  ]
}
```

### Field rules

| Field | Type | Required | Notes |
|---|---|---|---|
| `doctorId` | Guid string | per section | required **only when** `doctorValue` is sent; must be a **Doctor entity id** |
| `clinicId` | Guid string | if no doctor | required when `doctorId` is omitted; ignored when `doctorId` is sent (clinic is derived from the doctor) |
| `doctorValue` | int | per section | 1–5, must be provided if `doctorId` is sent |
| `clinicValue` | int | ✅ | 1–5 |
| `cleanlinessValue` | int | ✅ | 1–5 |
| `review` | string | optional | **ONE general review** shared across all sections, max 1000 chars |

### Rules & duplicates

- All three rows are created **together or not at all** (single transaction).
- **Only after a completed visit**: the backend requires at least one **completed appointment** (`AppointmentStatus.Completed`) booked by the same user for the rated doctor/clinic — rating without a completed visit returns HTTP 400 "لا يمكنك التقييم إلا بعد زيارة مكتملة" (`Ratings.NoCompletedVisit`). (Applies to the legacy `POST /ratings` too.)
- **Self-rating is blocked**: a doctor's own user account cannot rate their own profile (HTTP 400 "لا يمكنك تقييم نفسك").
- The clinic must be **active** (`IsActive = true`) — otherwise HTTP 404.
- The same patient may rate the **same doctor only once** and the **same clinic section only once** — a second attempt returns HTTP 400 "لقد قمت بتقييم هذا العنصر مسبقاً" (`Ratings.AlreadyRated`) and **nothing** is saved.
- There is **no update or delete endpoint** — if already rated, show the "already rated" state instead of the form.

---

## 🧾 Legacy Endpoint — `POST {base}/api/v1/ratings` (one section per call)

Kept for backward compatibility with old builds. If `type` is **omitted**, the backend infers it:

```
IF doctorId != null  → type = 1 (Doctor)
ELSE                 → type = 2 (Clinic)
```

The new app should **always use `/ratings/batch`** instead.

---

## ❌ Error Responses

All errors are wrapped in `ApiResponse<T>` and localized by the `Accept-Language` request header (send `ar` for Arabic):

```json
{
  "success": false,
  "message": "لقد قمت بتقييم هذا العنصر مسبقاً",
  "statusCode": 400
}
```

| HTTP | Localization key | Arabic message |
|---|---|---|
| 400 | `Ratings.TargetRequired` | يجب تحديد طبيب أو عيادة للتقييم |
| 400 | `Ratings.InvalidValue` | قيمة التقييم يجب أن تكون بين 1 و 5 |
| 400 | `Ratings.DoctorValueRequired` | يجب تحديد قيمة تقييم الطبيب عند تقييم طبيب |
| 400 | `Ratings.DoctorTargetRequired` | يجب تحديد الطبيب عند إرسال قيمة تقييم للطبيب |
| 400 | `Ratings.AlreadyRated` | لقد قمت بتقييم هذا العنصر مسبقاً |
| 400 | `Ratings.NoCompletedVisit` | لا يمكنك التقييم إلا بعد زيارة مكتملة |
| 400 | `Ratings.CannotRateSelf` | لا يمكنك تقييم نفسك |
| 404 | `DoctorMessages.NotFound` | الطبيب غير موجود |
| 404 | `ClinicMessages.ClinicNotFound` | العيادة غير موجودة (أو غير فعّالة) |
| 401 | — | missing/expired bearer token |
| 429 | — | rate limited (max 10 rating requests/min/IP) |

---

## 🧭 Where Do `doctorId` / `clinicId` Come From?

- **`clinicId`** — the id of the clinic the patient visited (already available in the booking/appointment object).
- **`doctorId`** — the **Doctor entity id**, NOT the user id. It is the same `id` the patient gets from:
  - the booking confirmation (`appointment.doctorId`), or
  - the doctor's public profile (`GET /doctors/{doctorId}/details` — the same id used in the URL).

Never try to convert a user id into a doctor id client-side — they are different.

---

## 🧰 Model (Kotlin / Swift)

```kotlin
data class SubmitClinicRatingsRequest(
    val doctorId: String?,     // optional — omit to skip the doctor section
    val clinicId: String?,     // required only when doctorId is null
    val doctorValue: Int?,     // 1..5, required when doctorId is sent
    val clinicValue: Int,      // 1..5
    val cleanlinessValue: Int, // 1..5
    val review: String?        // ONE general review shared across all sections (max 1000)
)

data class RatingDto(
    val id: String,
    val type: Int,
    val userId: String,
    val userName: String?,
    val doctorId: String?,
    val clinicId: String?,
    val value: Int,
    val review: String?,
    val createdAt: String
)
```

> ⚠️ Do **not** mark `userName` / `review` / `doctorId` / `clinicId` as non-null — the JSON always contains the keys but the values can be `null`.

---

## 📱 Suggested UX Flow

1. After the visit is marked **completed** → show the rating sheet (one screen, 3 sections).
2. Each section = star selector (1–5). The general review field (one text box) appears **after all sections**.
3. On submit → **one** `POST /ratings/batch` call with all three values + the review.
4. On success (201) → thank-you screen; the three rows are created server-side.
5. On `AlreadyRated` (400) → show "لقد قمت بتقييم هذا العنصر مسبقاً" and switch to read-only (no retry).
6. Optional: read back ratings with the GET endpoints to display the patient's own submission.

---

## 🚫 What the Mobile App Does NOT Do

- ❌ No update / delete of ratings — the backend has **no** such endpoints.
- ❌ No creating doctors/clinics — ids come from existing booking/profile data.
- ❌ No rating without authentication — the token is required.
- ❌ No string enum values — always send `type`/values as int (`1`/`2`/`3`).

---

## ✅ Mobile Checklist

- [ ] Send `Accept-Language: ar` (or `en`) on all rating calls for localized messages.
- [ ] Use `POST /ratings/batch` for the full rating sheet (one call, one general review).
- [ ] Only show the rating sheet **after a completed visit** (the backend rejects ratings without a completed appointment with 400 `NoCompletedVisit`).
- [ ] Validate client-side: values 1–5, `review` ≤ 1000 chars.
- [ ] Handle HTTP 400 `AlreadyRated` → show the "already rated" state, no retry.
- [ ] Handle HTTP 400 `NoCompletedVisit` / `CannotRateSelf` as validation errors.
- [ ] Handle 401 → re-auth flow; handle 429 → back off and retry with delay.
- [ ] Use the **Doctor entity id** (from booking/profile) — not the user id.
- [ ] If the visit had no doctor, omit `doctorId`/`doctorValue` and send `clinicId`.
