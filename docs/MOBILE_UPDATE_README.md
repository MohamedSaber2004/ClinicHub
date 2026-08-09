# Mobile App — Latest Update README (Ratings + Auth)

> This README documents the **last change pushed to GitHub** that affects the **mobile application**:
>
> - Commit: `d0889f2` (`savechanges`)
> - Date: Aug 9, 2026
> - Branch: `main` (pushed to `origin` — `github.com/MohamedSaber2004/ClinicHub`)
>
> Two areas changed: **Ratings** (new rating types + a new endpoint) and **Auth** (a new `doctorId` field in the login response). The mobile app must adjust to both.

---

## 1. Summary of Changes

| Area | Change | Mobile Impact |
|---|---|---|
| Ratings | New `RatingType` enum: `1` Doctor, `2` Clinic, `3` PlaceCleanliness (DB migration `20260809175628_AddRatingTypeToRatings`) | Rating submissions must send `type` as an integer |
| Ratings | `POST /api/v1/ratings` now accepts `type`; invalid values → 400 | Update rating request models & validation |
| Ratings | New endpoint `GET /api/v1/clinics/{clinicId}/place-cleanliness-ratings` | Read-only, mainly for web dashboards |
| Ratings | `RatingDto` response now includes `type` | Optional in app models |
| Auth | `AuthResponseDto` adds `doctorId` (nullable) | New JSON key in all auth responses; model it as **nullable** |

---

## 2. Ratings

The full integration guide already exists and covers everything the patient app needs to submit ratings:

- **[`docs/RATINGS_MOBILE_GUIDE.md`](RATINGS_MOBILE_GUIDE.md)** — endpoint contracts, request/response examples, Kotlin/Swift models, error table, UX flow, checklist.

Quick recap of what changed in this commit:

### `type` field (int enum)

`POST {base}/api/v1/ratings` now takes an optional `type`:

| `type` | Meaning | Target field |
|---|---|---|
| `1` | Doctor rating | `doctorId` |
| `2` | Clinic rating | `clinicId` |
| `3` | Place cleanliness | `clinicId` |

- **Send `type` as an integer** (`1` / `2` / `3`). The backend uses default JSON enum serialization — there is **no** `JsonStringEnumConverter`. Sending `"type": "Clinic"` fails with 400.
- If `type` is **omitted** (old clients), the backend infers it: `doctorId != null` → `1`, otherwise → `2`.
- `type: 0` or any value outside 1–3 → **400**.
- Per-type target validation: `type = 1` requires `doctorId` and null `clinicId`; `type = 2`/`3` require `clinicId` and null `doctorId`. Sending both → **400**.
- One rating per (user, type, target): a second attempt → **400** `لقد قمت بتقييم هذا العنصر مسبقاً` (`Ratings.AlreadyRated`).

### New endpoint

```
GET {base}/api/v1/clinics/{clinicId}/place-cleanliness-ratings
```

Lists cleanliness ratings (`type = 3`) for a clinic. Read-only — mainly used by the web dashboard.

### Response change

`RatingDto` now includes `type` (integer). The patient app's read models should add it as an optional field.

---

## 3. Auth — New `doctorId` Field

`AuthResponseDto` gained a new field:

```
Guid? DoctorId
```

It is serialized as `"doctorId"` in the JSON response, positioned **after `clinicId`** and **before `profilePictureUrl`**.

### Where it comes from

The backend resolves it from the Doctor entity linked to the logged-in user (`Doctor.UserId == user.Id`, not deleted). It is the **Doctor entity id** — not a user id.

### Which endpoints return it

| Endpoint | Returns `doctorId` |
|---|---|
| `POST /api/v1/auth/login` | Yes (when a doctor entity exists for the user) |
| `POST /api/v1/auth/login-web` | Yes |
| `POST /api/v1/auth/login-facebook` | Yes |
| `POST /api/v1/auth/login-google` | Yes |
| `POST /api/v1/auth/complete-facebook-registration` | Yes |
| `POST /api/v1/auth/signup` | Always `null` (no doctor entity at signup) |

### Example response

```json
{
  "accessToken": "...",
  "refreshToken": "...",
  "fullName": "محمد أحمد",
  "email": "m@example.com",
  "roles": "Patient",
  "id": "2f9a0000-0000-0000-0000-000000000004",
  "clinicId": null,
  "doctorId": null,
  "profilePictureUrl": null,
  "isFreelanceDoctor": false
}
```

### Why it matters for the mobile app

- The **doctor id is exactly what the ratings feature needs** for a `type = 1` (doctor) submission. Previously the app had to pull it from the booking or the doctor profile; now it is available directly in the login response.
- `doctorId` is `null` for patients, and for doctors who have not yet onboarded — **do not treat it as always present**.

### Mobile model change (required)

Add `doctorId` as a **nullable** field to the auth response model (Kotlin: `val doctorId: String? = null`; Swift: `String?`). Do **not** make it required — some endpoints always return `null`, and strict parsers that require every field can break auth entirely.

---

## 4. Mobile Impact Checklist

### Auth

- [ ] Add `doctorId: String?` (nullable) to the login/social-login response model in the app.
- [ ] Do not crash if `doctorId` is `null` — expected for patients and un-onboarded doctors.
- [ ] If the app stores auth data locally, persist `doctorId` for later use (e.g., rating submission).

### Ratings

- [ ] Send `Accept-Language: ar` (or `en`) on rating calls for localized error messages.
- [ ] Send `type` as **Int** (`1`/`2`/`3`) — never a string.
- [ ] Doctor rating section → `{ type: 1, doctorId, value, review }` — prefer the `doctorId` from the login response.
- [ ] Clinic section → `{ type: 2, clinicId, value, review }`; cleanliness section → `{ type: 3, clinicId, value, review }`.
- [ ] Validate client-side: `value` 1–5, `review` ≤ 1000 chars.
- [ ] Handle HTTP 400 `Ratings.AlreadyRated` → show "already rated" state, no retry.
- [ ] Never send both `doctorId` and `clinicId` in the same call.
- [ ] Do not rely on the `type` inference — always send `type` explicitly.

---

## 5. Pitfalls / Notes

- **`type` is an int, not a string** — the most common integration mistake; it causes a 400.
- **`doctorId` ordering in JSON** — irrelevant for JSON parsers, but if the app builds the model from an indexed/positional mapping, update it (new field sits between `clinicId` and `profilePictureUrl`).
- The new `place-cleanliness-ratings` GET endpoint is read-only and primarily for web dashboards; the patient app does **not** need it to submit ratings.
- No update/delete rating endpoints exist — once rated, the state is final.
