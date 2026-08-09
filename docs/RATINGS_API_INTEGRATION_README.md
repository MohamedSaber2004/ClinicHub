# 📱 Ratings API — Mobile Integration README

> Integration guide for the **patient mobile app**: how to submit and read the **4 rating types** through the ClinicHub Ratings API.

---

## 🧮 Rating Types

The API models **four** rating types. `type` is always sent as an **integer** (never a string).

| `type` | Rating type | Targets | Target field |
|---|---|---|---|
| `1` | **Doctor** rating | a doctor | `doctorId` |
| `2` | **Clinic** (general) rating | a clinic | `clinicId` |
| `3` | **Place cleanliness** rating | a clinic | `clinicId` |
| `4` | **Reception** rating | a clinic | `clinicId` |

Each rating row = one user + one target + one type (unique per user per target per type — a user can rate each section **once**).

---

## 🔐 Common Requirements (all calls)

- **Base URL:** `https://<host>/api/v1`
- **Auth:** `Authorization: Bearer <JWT>` — required on every rating endpoint (any authenticated user). `401` if missing/expired.
- **Language:** `Accept-Language: ar` (or `en`) to localize response messages.
- **Content-Type:** `application/json` for POST bodies.
- **Value range:** every rating `value` must be `1..5`.

---

## 📡 Endpoints Overview

| Method | Endpoint | Type |
|---|---|---|
| `POST` | `/ratings/batch` | all 4 (single call) |
| `POST` | `/ratings` | any single type |
| `GET` | `/doctors/{doctorId}/ratings` | `1` Doctor |
| `GET` | `/clinics/{clinicId}/ratings` | `2` Clinic |
| `GET` | `/clinics/{clinicId}/reception-ratings` | `4` Reception |
| `GET` | `/clinics/{clinicId}/place-cleanliness-ratings` | `3` Place cleanliness |

---

## ✍️ POST — Submit Ratings

### Option A (recommended): `POST /ratings/batch` — all sections in one call

One transaction that creates **up to 4 rows** with the **same shared review text**.

```json
{
  "doctorId": "3f9a0000-0000-0000-0000-000000000001",
  "clinicId": null,
  "doctorValue": 5,
  "clinicValue": 4,
  "receptionValue": 4,
  "cleanlinessValue": 5,
  "review": "تجربة ممتازة، دكتور راقي والاستقبال محترم"
}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| `doctorId` | Guid | per section | required **only when** `doctorValue` is sent |
| `clinicId` | Guid | if no doctor | required when `doctorId` omitted; derived from the doctor otherwise |
| `doctorValue` | int | per section | 1–5; must be present if `doctorId` is sent |
| `clinicValue` | int | ✅ | 1–5 |
| `receptionValue` | int | ✅ | 1–5 |
| `cleanlinessValue` | int | ✅ | 1–5 |
| `review` | string | optional | one review shared across all created rows, max 1000 chars |

- Rows created: **Clinic (2) + Reception (4) + Cleanliness (3)** always; **Doctor (1)** only when `doctorId`/`doctorValue` are sent.
- If any section was already rated → `400` and **nothing** is saved.

### Option B (single): `POST /ratings` — one row per call

```json
{
  "type": 4,
  "clinicId": "4a1c0000-0000-0000-0000-000000000002",
  "value": 4,
  "review": "استقبال جيد"
}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| `type` | int | optional | `1`–`4`. If omitted: `doctorId` present → `1` (Doctor), else → `2` (Clinic) |
| `doctorId` | Guid | if `type=1` | must be a **Doctor entity id**; `clinicId` must be null |
| `clinicId` | Guid | if `type=2,3,4` | `doctorId` must be null |
| `value` | int | ✅ | 1–5 |
| `review` | string | optional | max 1000 chars |

---

## 📖 GET — Read Ratings (per type)

All GETs return `ApiResponse<List<RatingDto>>` with HTTP `200`. If the target does not exist → `404`.

### 1. Doctor ratings — `GET /doctors/{doctorId}/ratings`

```json
{
  "success": true,
  "data": [
    {
      "id": "7d1c0000-0000-0000-0000-000000000009",
      "type": 1,
      "userId": "2f9a0000-0000-0000-0000-000000000004",
      "userName": "محمد أحمد",
      "doctorId": "3f9a0000-0000-0000-0000-000000000001",
      "clinicId": null,
      "value": 5,
      "review": "دكتور ممتاز",
      "createdAt": "2026-08-09T21:10:00"
    }
  ]
}
```

### 2. Clinic (general) ratings — `GET /clinics/{clinicId}/ratings`

Same shape, rows with `"type": 2`, `clinicId` set, `doctorId: null`.

### 3. Reception ratings — `GET /clinics/{clinicId}/reception-ratings`

Same shape, rows with `"type": 4`.

### 4. Place cleanliness ratings — `GET /clinics/{clinicId}/place-cleanliness-ratings`

Same shape, rows with `"type": 3`.

> **Averages:** the backend returns the clinic/doctor **average rating + total count** in the mobile detail endpoints (`GET /clinics/{clinicId}/details` / `GET /doctors/{doctorId}/details` → `averageRating`, `totalRatings`). The GET lists above are for showing individual reviews — no client-side averaging needed.

---

## 📦 Response Envelope

Every endpoint wraps data in `ApiResponse<T>`:

```json
{
  "success": true,
  "message": "تم إرسال التقييم بنجاح",
  "data": { },
  "errors": {},
  "statusCode": 201
}
```

- POST success: `201` (`data` = created `RatingDto` / `List<RatingDto>`)
- GET success: `200` (`data` = `List<RatingDto>`)

`RatingDto`:

| Field | Type | Notes |
|---|---|---|
| `id` | Guid | rating row id |
| `type` | int | `1` Doctor, `2` Clinic, `3` Cleanliness, `4` Reception |
| `userId` | Guid | rater user id |
| `userName` | string? | rater full name (can be `null`) |
| `doctorId` | Guid? | set only for type `1` |
| `clinicId` | Guid? | set only for types `2`,`3`,`4` |
| `value` | int | 1–5 |
| `review` | string? | can be `null` |
| `createdAt` | DateTime | ISO 8601 |

---

## ❌ Error Responses

```json
{
  "success": false,
  "message": "لقد قمت بتقييم هذا العنصر مسبقاً",
  "statusCode": 400
}
```

| HTTP | Scenario |
|---|---|
| `400` | invalid value (not 1–5), missing target, rating without a **completed visit**, self-rating, already rated |
| `404` | doctor/clinic not found or clinic inactive |
| `401` | missing/expired token |
| `429` | rate limited (max 10 rating requests/min/IP) |

---

## 🧰 Model (Kotlin)

```kotlin
data class SubmitVisitRatingsRequest(
    val doctorId: String?,      // optional — omit to skip the doctor section
    val clinicId: String?,      // required only when doctorId is null
    val doctorValue: Int?,      // 1..5, required when doctorId is sent
    val clinicValue: Int,       // 1..5
    val receptionValue: Int,    // 1..5
    val cleanlinessValue: Int,  // 1..5
    val review: String?         // one review shared across all sections
)

data class CreateRatingRequest(
    val type: Int?,             // 1..4 (inferred if omitted)
    val doctorId: String?,      // type 1
    val clinicId: String?,      // types 2, 3, 4
    val value: Int,             // 1..5
    val review: String?
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

data class ApiResponse<T>(
    val success: Boolean,
    val message: String?,
    val data: T?,
    val errors: Map<String, List<String>>,
    val statusCode: Int
)
```

> ⚠️ Do **not** mark `userName` / `review` / `doctorId` / `clinicId` as non-null — the JSON always contains the keys but the values can be `null`.

---

## ✅ Mobile Checklist

- [ ] Send `Authorization: Bearer <JWT>` on every rating call.
- [ ] Send `Accept-Language: ar` (or `en`) for localized messages.
- [ ] Submit the full rating sheet with **one** `POST /ratings/batch` call (doctor + clinic + reception + cleanliness + one review).
- [ ] Only show the rating sheet **after a completed visit** (backend returns 400 `NoCompletedVisit` otherwise).
- [ ] Validate client-side: every value 1–5, `review` ≤ 1000 chars.
- [ ] Use the **Doctor entity id** (from booking/profile) — not the user id.
- [ ] If the visit had no doctor: omit `doctorId`/`doctorValue` and send `clinicId`.
- [ ] Handle `400 AlreadyRated` → show the "already rated" state, no retry.
- [ ] Handle `401` → re-auth; `429` → back off and retry with delay.
- [ ] For reviews lists, call the matching GET per type (`1`→doctor, `2`→clinic, `3`→cleanliness, `4`→reception).
