# Deeplink Store-Redirect — Backend Implementation & Mobile Guide

> Smart deeplink flow: one unique URL per destination. If the Flutter app is installed, the OS opens the app on the right screen. If it is **not** installed, the browser opens the ClinicHub server, which serves an HTML+JS page that redirects the user to **Google Play** (Android) or the **App Store** (iOS) — or to the web app on desktop.

---

## 1. How It Works

```
Link: https://doctory.runasp.net/go/{unique-path}
  |
  |-- App installed -> OS App Links / Universal Links -> Flutter app opens -> navigates to {unique-path}
  |
  |-- App NOT installed -> browser -> GET /go/{**path} on ClinicHub API
        -> serves a branded Arabic (RTL) HTML page
        -> JS:
             Mobile  -> tries to open the app via  clinichub://{path}
                        (if it opens, page is backgrounded and the timer is cancelled)
                        else after 1.8s -> redirects to the correct store
             Desktop -> shows the page with buttons (Open app / Google Play / App Store / Web) — no auto-redirect
        -> the page also shows manual buttons: Open app / Google Play / App Store / Web
```

- The `{unique-path}` is a **pass-through** — the server does not interpret it. It is used only to:
  - open the right screen inside the app (`clinichub://{path}`), and
  - preserve the destination so the app can navigate after being opened.
- Path validation: only `a-z A-Z 0-9 - _ . /`, max 256 chars, HTML-encoded in the page.

## 2. Endpoint

| Method | Route | Auth | Response |
|---|---|---|---|
| `GET` | `{base}/go/{**path}` | None (anonymous) | `text/html` fallback page |

Examples:

- `https://doctory.runasp.net/go/clinic/3f9a0000-0000-0000-0000-000000000001`
- `https://doctory.runasp.net/go/post/abc-123`
- Invalid path (bad chars / too long) → **404**.

## 3. Server Configuration (`appsettings.*.json` → `DeepLinkSettings`)

| Key | Default | Purpose |
|---|---|---|
| `AppScheme` | `clinichub` | Custom scheme used to open the app (`clinichub://{path}`) |
| `AndroidPackageName` | `com.doctory` | Play Store package |
| `PlayStoreUrl` | `https://play.google.com/store/apps/details?id=com.doctory` | Android store redirect |
| `AppStoreUrl` | `https://apps.apple.com/app/idYOUR_APPLE_APP_ID` | iOS store redirect — **fill real Apple ID** |
| `BaseUrl` | API host (e.g. `https://doctory-icare.runasp.net`) | Host for generated `/go/...` links |
| `WebFallbackUrl` | frontend URL | Desktop redirect target |
| `AppNameAr` / `AppNameEn` | كلينيك هب / ClinicHub | Page branding |

## 4. Generating Links Server-Side

Use `IDeepLinkService.GenerateGoLink` — it points to the **API host** (`DeepLinkSettings.BaseUrl`), not the Flutter web host:

```csharp
var link = _deepLinkService.GenerateGoLink("clinic/3f9a0000-0000-0000-0000-000000000001");
// => https://doctory-icare.runasp.net/go/clinic/3f9a0000-0000-0000-0000-000000000001
```

> Do NOT use `GenerateLink` for `/go/...` links — it builds against `FrontendUrl` (the Flutter web host), where the fallback page does not exist.

## 5. Flutter / Mobile Setup (to make links open the app)

### Android
1. Android App Links (production): `assetlinks.json` is already served at `/.well-known/assetlinks.json` (package `com.doctory`). Upload the app with a signed release keystore whose SHA-256 fingerprint is the one in that file, and verify with the Play Console.
2. Intent filter in `AndroidManifest.xml`:
   ```xml
   <intent-filter android:autoVerify="true">
     <action android:name="android.intent.action.VIEW"/>
     <category android:name="android.intent.category.DEFAULT"/>
     <category android:name="android.intent.category.BROWSABLE"/>
     <data android:scheme="https" android:host="doctory.runasp.net"/>
     <data android:scheme="clinichub"/>
   </intent-filter>
   ```
3. Use `app_links` package:
   ```dart
   final links = AppLinks();
   final uri = await links.getInitialLink();       // cold start
   links.uriLinkStream.listen(_handle);            // warm start
   ```

### iOS
1. `Info.plist` → Associated Domains: `applinks:doctory.runasp.net` + URL scheme `clinichub`.
2. The server already hosts `/.well-known/apple-app-site-association`. **The `appID` in it is still the placeholder** — the mobile team must replace `YOUR_TEAM_ID.com.yourcompany.clinichub` with the real Team ID + bundle id (and add any missing paths) before release.
3. Parse `clinichub://{path}` in the app and map path → screen (e.g. `clinic/{id}` → ClinicDetailsScreen).

## 6. Testing

```bash
# Desktop -> page with buttons (no auto-redirect)
curl "https://localhost:5027/go/test"
# Android UA -> page with Play Store target + clinichub attempt
curl -A "Mozilla/5.0 (Linux; Android 13) Chrome/120.0 Mobile" "https://localhost:5027/go/clinic/abc"
# iOS UA -> App Store target
curl -A "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X)" "https://localhost:5027/go/clinic/abc"
```

## 7. Notes / Limitations

- **Deferred deep links** (auto-open the target screen after a fresh install) are NOT implemented — the store redirect does not carry the path back into the app after installation. Phase 2 idea: pass `?path=...` to the store, or use a first-launch endpoint the app polls.
- The iOS App Store link and the AASA `appID` need real values before production.
- The page has no analytics; add optional click tracking later if needed.
