# Deep Link Integration — Mobile App

## Overview

Deep links are **web URLs** based on `EmailSettings.FrontendUrl` (e.g. `https://clinicHub.app/...`).  
The mobile app intercepts them via **universal links** (Android App Links / iOS Universal Links).

---

## URL Formats

| Scenario | URL Pattern |
|----------|-------------|
| Clinic approval | `{FrontendUrl}/clinic/setup?clinicId={guid}&userId={guid}&token={hmac}` |
| Post sharing | `{FrontendUrl}/post/{postId}` |
| Verification approved | `{FrontendUrl}/auth/verification-approved?userId={id}&role={role}&status={status}&token={hmac}` |

> **Note**: Push notifications also carry a `link` field in the notification `Data` payload (key: `"link"`) for navigation. The mobile app should read this field on notification tap.

---

## 1. Platform Configuration

Register the `FrontendUrl` domain for universal links on both platforms:

- **Android**: `AndroidManifest.xml` with HTTPS intent-filter + `assetlinks.json` on your server
  - Template: `{FrontendUrl}/.well-known/assetlinks.json`
- **iOS**: `Associated Domains` entitlement (`applinks:yourdomain.com`) + `apple-app-site-association` on your server
  - Template: `{FrontendUrl}/.well-known/apple-app-site-association`

> The `.well-known` files are served statically from `wwwroot/.well-known/`. Update the placeholder values in these files with your actual app package name, SHA256 fingerprint, and Team ID.

---

## 2. Intercept the URL

When the app receives an incoming URL, parse the path and query parameters:

| Path | Purpose |
|------|---------|
| `/clinic/setup` | Clinic owner approval — navigate to clinic setup form |
| `/post/{postId}` | Shared post — navigate to post details |

---

## 3. Verify HMAC Token (Clinic Approval)

The `token` parameter is an HMAC-SHA256 hex string. **The mobile should verify the token before trusting the link.**

### Option A: Server-side verification (recommended)

```
POST {FrontendUrl}/api/v1/deep-links/verify
Body: { "data": "clinic-approval:{clinicId}:{userId}", "token": "{hmac}" }
Response: { "valid": true }
```

### Option B: Client-side verification

```
HMAC-SHA256(key: DeepLinkSecret, data: "clinic-approval:{clinicId}:{userId}")
```

Compare the computed HMAC hex string with the `token` query param. If they match, the link is authentic.

> **Important**: The `DeepLinkSecret` must be shared securely between the backend and mobile app (e.g. injected via CI/CD, not hardcoded). Prefer server-side verification to avoid exposing the secret in the client.

---

## 4. Routing Logic

- **Clinic approval**: route to clinic setup/registration form, pass `clinicId` and `userId`
- **Post share**: route to post details view using `postId` — fetch full post data via `GET /api/v1/posts/{postId}` if needed

---

## 5. Sharing Posts (Mobile → External)

When a user shares a post, construct the link locally:

```
{FrontendUrl}/post/{postId}
```

Or fetch it from the API:

```
GET {FrontendUrl}/api/v1/posts/{postId}/share-link

Response: { "link": "https://clinicHub.app/post/..." }
```

---

## Environment Configuration

The `FrontendUrl` and `DeepLinkSecret` values differ per environment (Development / Test / Production). Ensure the mobile app uses the correct matching values for each environment.
