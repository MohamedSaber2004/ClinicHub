# 📱 Ratings — Mobile App Integration Guide (Patient App)

> What the **patient mobile app** needs to do to let patients rate their visit. The backend is **fully implemented** — the mobile team only needs to **consume** the endpoints below. All submission happens in the patient app; viewing/managing happens on the web dashboards (read-only).

---

## 🎯 What the Mobile App Must Do

After a patient completes a visit, show a **rating sheet with 3 sections** — each section submits its **own separate row**:

| Section | `type` | Target field | Example |
|---|---|---|---|
| 1. تقييم الطبيب (Doctor rating) | `1` | `doctorId` | `"type": 1, "doctorId": "..."` |
| 2. تقييم العيادة (Clinic rating) | `2` | `clinicId` | `"type": 2, "clinicId": "..."` |
| 3. نظافة المكان (Place cleanliness) | `3` | `clinicId` | `"type": 3, "clinicId": "..."` |

Each section: **stars (1–5)** + **optional review text** (max 1000 chars).

> ⚠️ **`type` is an integer, not a string** — the backend uses default JSON enum serialization (`1` / `2` / `3`), there is **no** `JsonStringEnumConverter`. Sending `"type": "Clinic"` fails with a 400.

---

## 📡 Endpoints

| Method | Endpoint | Auth | Purpose |
|---|---|---|---|
| `POST` | `{base}/api/v1/ratings` | ✅ Bearer (any authenticated role) | Submit one rating row |
| `GET` | `{base}/api/v1/doctors/{doctorId}/ratings` | ✅ Bearer | List doctor ratings (type=1) |
| `GET` | `{base}/api/v1/clinics/{clinicId}/ratings` | ✅ Bearer | List clinic ratings (type=2) |
| `GET` | `{base}/api/v1/clinics/{clinicId}/place-cleanliness-ratings` | ✅ Bearer | List cleanliness ratings (type=3) |

> The GET endpoints are mainly used by the web dashboard. The patient app only **needs** `POST /ratings`; the doctor/clinic averages are already returned by the existing mobile doctor-details endpoint (`GET /doctors/{doctorId}/details` → `averageRating`).

---

## 🧾 Create a Rating — `POST {base}/api/v1/ratings`

**Request:**
```json
{
  "type": 1,
  "doctorId": "3f9a0000-0000-0000-0000-000000000001",
  "clinicId": null,
  "value": 5,
  "review": "دكتور ممتاز والتعامل راقي"
}
```

**Response (`ApiResponse<RatingDto>`, HTTP 201):**
```json
{
  "success": true,
  "message": "تم إرسال التقييم بنجاح",
  "data": {
    "id": "7d1c0000-0000-0000-0000-000000000009",
    "type": 1,
    "userId": "2f9a0000-0000-0000-0000-000000000004",
    "userName": "محمد أحمد",
    "doctorId": "3f9a0000-0000-0000-0000-000000000001",
    "clinicId": null,
    "value": 5,
    "review": "دكتور ممتاز والتعامل راقي",
    "createdAt": "2026-08-09T20:15:00"
  }
}
```

### Field rules

| Field | Type | Required | Notes |
|---|---|---|---|
| `type` | int | ⚠️ see below | `1` Doctor, `2` Clinic, `3` PlaceCleanliness |
| `doctorId` | Guid string | per type | required when `type = 1`; must be a **Doctor entity id** (see "Where do the ids come from?") |
| `clinicId` | Guid string | per type | required when `type = 2` or `3` |
| `value` | int | ✅ | 1–5 (invalid → 400) |
| `review` | string | optional | max 1000 chars |

### The `type` field is optional (backward compatibility)

If `type` is **omitted** (old clients), the backend infers it:

```
IF doctorId != null  → type = 1 (Doctor)
ELSE                 → type = 2 (Clinic)
```

The new app should **always send `type` explicitly** — the inference is only for old builds. `type: 0` or any value outside 1–3 → **400**.

### Per-type target validation

- `type = 1` → `doctorId` required, `clinicId` must be null.
- `type = 2` or `type = 3` → `clinicId` required, `doctorId` must be null.
- Sending both `doctorId` + `clinicId` → **400**.

### One rating per (user, type, target)

A patient can rate the **same doctor only once** — a second attempt returns HTTP 400:

> "لقد قمت بتقييم هذا العنصر مسبقاً" (`Ratings.AlreadyRated`)

The clinic and cleanliness are **separate rows**: the same patient may rate the clinic (type 2) **and** the cleanliness (type 3) — one each. There is **no update or delete endpoint** — if a user already rated, show the "already rated" state instead of the form.

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
| 400 | `Ratings.SingleTargetRequired` | لا يمكن تقييم طبيب وعيادة في نفس الوقت |
| 400 | `Ratings.InvalidValue` | قيمة التقييم يجب أن تكون بين 1 و 5 |
| 400 | `Ratings.AlreadyRated` | لقد قمت بتقييم هذا العنصر مسبقاً |
| 404 | `DoctorMessages.NotFound` | الطبيب غير موجود |
| 404 | `ClinicMessages.ClinicNotFound` | العيادة غير موجودة |
| 401 | — | missing/expired bearer token |

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
data class RatingRequest(
    val type: Int,          // 1 = Doctor, 2 = Clinic, 3 = PlaceCleanliness
    val doctorId: String?,  // required when type == 1
    val clinicId: String?,  // required when type == 2 or 3
    val value: Int,         // 1..5
    val review: String?     // max 1000 chars
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
2. Each section = star selector (1–5) + optional note field.
3. Submit each section as its **own** `POST /ratings` call — or send all three sequentially and handle failures per-section.
4. On `AlreadyRated` (400) → show "لقد قمت بتقييم هذا العنصر مسبقاً" and switch that section to read-only (no retry).
5. Optionally read back ratings with the GET endpoints to display the patient's own submission.

---

## 🚫 What the Mobile App Does NOT Do

- ❌ No update / delete of ratings — the backend has **no** such endpoints.
- ❌ No creating doctors/clinics — ids come from existing booking/profile data.
- ❌ No rating without authentication — the token is required.
- ❌ No string enum values — always send `type` as int (`1`/`2`/`3`).

---

## ✅ Mobile Checklist

- [ ] Send `Accept-Language: ar` (or `en`) on all rating calls for localized messages.
- [ ] Model `type` as **Int** (`1`/`2`/`3`) — never string enum.
- [ ] Section 1 → `{ type: 1, doctorId, value, review }` (from appointment/booking data).
- [ ] Section 2 → `{ type: 2, clinicId, value, review }`.
- [ ] Section 3 → `{ type: 3, clinicId, value, review }`.
- [ ] Validate client-side: `value` 1–5, `review` ≤ 1000 chars.
- [ ] Handle HTTP 400 `AlreadyRated` → show the "already rated" state, no retry.
- [ ] Handle 401 → re-auth flow.
- [ ] Never send both `doctorId` and `clinicId` in the same call.
- [ ] Use the **Doctor entity id** (from booking/profile) — not the user id.
