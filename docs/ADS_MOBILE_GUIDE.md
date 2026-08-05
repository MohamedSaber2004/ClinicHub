# 📱 Ads — Mobile App Integration Guide (Patient App)

> What the **patient mobile app** needs to do to display clinic ads. The backend is fully implemented — the mobile team only needs to **consume** the public endpoint below. Everything else (buying ads, uploading logos, payments) happens on the web dashboards, not in the patient app.

---

## 🎯 What the Mobile App Must Do

1. Call `GET {base}/api/v1/public/ads/active` (no auth) to fetch active ads.
2. Render each ad as a card/badge:
   - With logo → image (fallback label = clinic name).
   - Without logo → text badge (name + package + period).
3. Display the "نشط" (Active) tag.
4. (Optional) Tapping the ad can navigate to the clinic page — `clinicId` is provided.

---

## 📡 The Only Endpoint You Need

### `GET {base}/api/v1/public/ads/active`

- **Auth:** none (`AllowAnonymous`)
- **Returns:** only `status == 1` (Active) ads that are **not expired**, sorted by `endDate` (newest first)

**Response (wrapped in `ApiResponse`):**
```json
{
  "success": true,
  "data": [{
    "id": "5f2c0000-0000-0000-0000-000000000002",
    "clinicId": "2d1a0000-0000-0000-0000-000000000003",
    "clinicName": "مركز القلب التخصصي",
    "clinicLogoUrl": null,
    "imageUrl": "clinic-logo-8f3a.png",
    "packageId": "3f9a0000-0000-0000-0000-000000000001",
    "packageNameAr": "شريط مميز",
    "title": null,
    "startDate": "2026-08-05T00:00:00",
    "endDate": "2026-08-19T00:00:00"
  }]
}
```

| Field | Type | Notes |
|---|---|---|
| `id` | Guid | ad id |
| `clinicId` | Guid | use for navigation to the clinic |
| `clinicName` | string \| null | clinic display name |
| `clinicLogoUrl` | string \| null | **clinic's** profile logo (relative path) — not the ad logo |
| `imageUrl` | string \| null | **ad logo** (the one the clinic owner uploaded for this ad) — relative path |
| `packageId` | Guid | ad package id |
| `packageNameAr` | string \| null | package name (Arabic) — e.g. "شريط مميز" |
| `title` | string \| null | currently always `null` |
| `startDate` | string | ISO date-time |
| `endDate` | string | ISO date-time |

> ⚠️ **Both image fields are nullable** — the JSON always contains the keys, but the value can be `null`.

---

## 🖼️ Image URL Resolution

All image fields are **relative paths**. Full URL is always:

```
{base}/files/{imageUrl}
```

Example: `imageUrl = "clinic-logo-8f3a.png"` → `https://api.clinichub.com/files/clinic-logo-8f3a.png`

---

## 🧩 Rendering Logic (required contract)

For each ad:

```
IF ad.imageUrl != null:
    render logo image   →  Image(src = "{base}/files/{ad.imageUrl}")
    fallback label      →  ad.clinicName
    tag                 →  "نشط"

ELSE (imageUrl == null):
    render text badge:
      • Headline:   ad.clinicName
      • Sub-label:  ad.packageNameAr
      • Period:     ad.startDate → ad.endDate   (formatted YYYY-MM-DD)
    tag             →  "نشط"
```

**Reference look** (matches the web dashboard preview "معاينة الإعلان في تطبيق المرضى"):
Rounded-square badge — logo image (or clinic initial) on the right, name + package + period on the left, "نشط" tag.

---

## 🧰 Model (Kotlin / Swift)

```kotlin
data class PublicAd(
    val id: String,
    val clinicId: String,
    val clinicName: String?,
    val clinicLogoUrl: String?,
    val imageUrl: String?,
    val packageId: String,
    val packageNameAr: String?,
    val title: String?,
    val startDate: String,
    val endDate: String
)
```

> ⚠️ Do **not** mark `imageUrl` as non-null — the backend emits `null` when the clinic uploaded no logo, and a non-nullable field would crash Kotlin serialization.

---

## 🚫 What the Mobile App Does NOT Do

- ❌ No payment / checkout — buying ads happens on the **web dashboard** (Paymob hosted checkout).
- ❌ No uploading logos — done on the web dashboard.
- ❌ No creating / activating / deactivating ads — activation is done by the backend payment webhook.
- ❌ No status checks — the endpoint already filters to active ads only.

---

## 🔁 Polling / Refreshing (optional)

Ads expire when `endDate` passes (the endpoint excludes them automatically). To keep the list fresh:

- Refresh on app open + on clinic list screen focus (e.g. pull-to-refresh).
- No push notifications for ads are implemented — a periodic refresh is sufficient.

---

## ✅ Mobile Checklist

- [ ] Call `GET {base}/api/v1/public/ads/active` (anonymous) and parse `ApiResponse<PublicAd[]>`.
- [ ] Model `imageUrl` / `clinicLogoUrl` as **nullable**.
- [ ] Compose full URL as `{base}/files/{imageUrl}`.
- [ ] Render image when `imageUrl != null`; otherwise text badge (clinicName + packageNameAr + startDate → endDate).
- [ ] Show "نشط" tag on every ad.
- [ ] Handle empty list (no ads) gracefully.
- [ ] Navigate to clinic page on tap using `clinicId` (optional).
