# Paymob Integration IDs — Dashboard Mapping (2026-08-28 updated per Prompts/prompt.txt)

> Source: `Prompts/prompt.txt:1-2` + live Intention API test via `ClinicHub.Infrastructure/Services/Paymob/PaymobService.cs:116` — **corrected per user dashboard labels 2026-08-28 22:55**

## Dashboard (Paymob → Developers → Payment Integrations)

| Integration ID | Channel | Type | Created | Gateway (API) | Dashboard label | Purpose in ClinicHub (CORRECTED) |
|---|---|---|---|---|---|---|
| **5671290** | `online` | `VPC` | 17 May 2026 11:58 | `MIGS` (Mastercard VPC legacy) `201` `gateway_type=MIGS` | `wallets` + `بطاقة ائتمان` (credit card) | **Card** — `PaymobSettings.IntegrationId` → `PaymobService.cs:90` `InitiateCheckoutPaymentAsync`. Use for credit card UnifiedCheckout (via `clientSecret`). Note: legacy `post_pay` redirect in API response but still works with Intention API. |
| **5690721** | `online_new` | `UIG` | 26 May 2026 11:24 | `UIG` (Unified) `201` `gateway_type=UIG` | `محفظة` (wallet) | **Wallet** — `PaymobSettings.WalletIntegrationId` → `PaymobService.cs:59` `InitiateWalletPaymentAsync`. Use for Vodafone Cash / Orange / Etisalat. |
| **5690722** | — | — | — | `404 {"detail":"Integration ID/Name does not exist..."}` | — | **NOT FOUND** — old invalid `WalletIntegrationId` in configs before 2026-08-28 fix. Must not use. |

## Live test (Intention API `POST https://accept.paymob.com/v1/intention/` with `SecretKey=egy_sk_test_cdec...` / `PublicKey=egy_pk_test_65rj...`)

```
5671290 -> 201 gateway_type=MIGS order_id=597067288 redirection_url=post_pay  (card legacy)
5690721 -> 201 gateway_type=UIG  order_id=597067295 redirection_url=https://doctory-icare.runasp.net/api/v1/payments/result (wallet UIG)
5690722 -> 404 detail="Integration ID/Name does not exist"
```

## Why "credit card intent is not showing Paymob credit card payment"

Root cause: **IDs were swapped** in `ClinicHub.API/appsettings.Development.json:227-228` / `Test.json:215-216`:
- Before fix: `IntegrationId=5690721` (actually **wallet** per dashboard) → card flow called wallet gateway → UnifiedCheckout showed wallet phone input, not card form
- Before fix: `WalletIntegrationId=5690722` → **invalid** → wallet flow 404

After fix (local, gitignored, not committed — `.gitignore:367-368`):
```json
// ClinicHub.API/appsettings.Development.json:223-232 / Test.json:211-220
"PaymobSettings": {
  "IntegrationId": "5671290",       // credit card (VPC/MIGS) — corrected
  "WalletIntegrationId": "5690721"  // wallet (UIG) — corrected per dashboard محفظة
}
```
Both now `201`. Card intent now correctly uses `5671290` → UnifiedCheckout with card fields. Wallet intent uses `5690721` → wallet phone input.

Other pitfalls:
- `PaymentMethodMapper.cs:7` `ToEnum(null)->PaymobWallet` + `InitiatePaymentCommandHandler.cs:63` defaults omitted → wallet (now 5690721 valid) vs `InitiateBookingPaymentCommandHandler.cs:65` defaults omitted → card (5671290) — always send explicit `paymentMethod:"card"|"wallet"` to avoid default confusion.
- Production `appsettings.Production.json:173-179` still placeholders `YOUR_...` → `PaymobService.cs:34` now logs `ResolveIntegrationId` error instead of crashing.

## Verification

```powershell
dotnet run --project C:\Users\ALQasem\AppData\Local\Temp\opencode\PaymobTest
# expects 5671290 201 MIGS + 5690721 201 UIG
```

Test via API:
`POST /api/v1/payments/initiate { appointmentId, paymentMethod:"card" }` → `redirectUrl` UnifiedCheckout → card form
`POST ... { paymentMethod:"wallet" }` → UnifiedCheckout → wallet phone

## Code improvements (commit 5167292)

- `PaymobService.cs:19` inject `ILogger<PaymobService>`
- `PaymobService.cs:34` `ResolveIntegrationId` safe `TryParse` for placeholder `YOUR_...`
- `PaymobService.cs:182` logs Paymob body + surfaces `detail` so 404 is visible to client

## References

- `ClinicHub.Infrastructure/Services/Paymob/PaymobService.cs:116` `CreateIntentionAsync`
- `ClinicHub.Application/Features/AdminPayments/PaymentMethodMapper.cs:7`
- `AGENTS.md:62` — `appsettings.json` gitignored
