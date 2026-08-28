# Paymob Integration IDs — Dashboard Mapping (2026-08-28)

> Source: `Prompts/prompt.txt` + live Intention API test via `ClinicHub.Infrastructure/Services/Paymob/PaymobService.cs`

## Dashboard (Paymob → Developers → Payment Integrations)

| Integration ID | Channel | Type | Created | Gateway (API) | Currency | Purpose in ClinicHub |
|---|---|---|---|---|---|---|
| **5671290** | `online` | `VPC` | 17 May 2026 11:58 | `MIGS` (Mastercard Internet Gateway Service) — legacy VPC | EGP | **Legacy card** (`post_pay` redirect). Works with Intention API (201) but returns `redirection_url=https://accept.paymobsolutions.com/api/acceptance/post_pay` and `gateway_type=MIGS`. Use only if you need legacy iframe. |
| **5690721** | `online_new` | `UIG` | 26 May 2026 11:24 | `UIG` (Unified) — new Intention API | EGP | **Card — UnifiedCheckout** (`redirection_url` = your `PaymobSettings.RedirectionUrl`). This is `PaymobSettings.IntegrationId` in `appsettings.Development/Test.json:227/215` and used by `PaymobService.cs:90` `InitiateCheckoutPaymentAsync`. **Keep as card.** |
| **5690722** | — | — | — | — | — | **NOT FOUND** on dashboard → Intention API `404 {"detail":"Integration ID/Name does not exist..."}` (`PaymobTest` 2026-08-28). This is `WalletIntegrationId` in current configs. **Invalid — wallet payments will fail.** |

## Live test (Intention API `POST https://accept.paymob.com/v1/intention/` with `SecretKey=egy_sk_test_cdec...` / `PublicKey=egy_pk_test_65rj...`)

```
5671290 -> 201 gateway_type=MIGS order_id=597067288 redirection_url=post_pay
5690721 -> 201 gateway_type=UIG  order_id=597067295 redirection_url=https://doctory-icare.runasp.net/api/v1/payments/result
5690722 -> 404 detail="Integration ID/Name does not exist"
```

## Why "credit card intent is not showing Paymob credit card payment"

1. **Card (5690721 UIG) works** — UnifiedCheckout URL `https://accept.paymob.com/unifiedcheckout/?publicKey=...&clientSecret=...` correctly shows card form. If mobile sees empty/wallet instead, check `paymentMethod` param:
   - `PaymentMethodMapper.cs:7` `ToEnum(null) -> PaymobWallet` 
   - `InitiatePaymentCommandHandler.cs:63` defaults omitted → wallet (uses `WalletIntegrationId` → 404)
   - `InitiateBookingPaymentCommandHandler.cs:65` defaults omitted → card (uses `IntegrationId` → works)
   - Always send `paymentMethod:"card"|"credit_card"` for card flow.

2. **Wallet (5690722) invalid** — any wallet flow (`PaymobWallet`) calls `PaymobService.cs:59` `InitiateWalletPaymentAsync` with `WalletIntegrationId=5690722` → Paymob 404 → `PaymobService.cs:182` now logs `Failed: integrationId=5690722` and surfaces `PaymobOrderFailed: Integration ID does not exist` (fixed in `5167292`).

3. **5671290 is NOT a wallet** — it is also card (MIGS). Do not use it as `WalletIntegrationId`.

## Fix

1. **Paymob dashboard**: Create new Mobile Wallet integration:
   `Dashboard → Developers → Payment Integrations → New → Type: Mobile Wallet (Vodafone Cash / Orange / Etisalat) → Currency EGP → Save → Copy its ID`
2. Update local (gitignored) configs:
   ```json
   // ClinicHub.API/appsettings.Development.json:223-232
   // ClinicHub.API/appsettings.Test.json:211-220
   "PaymobSettings": {
     "IntegrationId": "5690721",        // card UIG — keep
     "WalletIntegrationId": "YOUR_NEW_WALLET_ID", // <-- replace 5690722
   }
   ```
   No commit needed (files are `.gitignore:367-368`). Production placeholders (`appsettings.Production.json:173-179`) must be set via env `PaymobSettings__IntegrationId` / `PaymobSettings__WalletIntegrationId` or `dotnet user-secrets`.

3. Verify in dashboard that `5690721` gateway shows `Card` and new wallet ID shows `Wallet`.

4. Test again with `PaymobTest` or via API:
   `POST /api/v1/payments/initiate { appointmentId, paymentMethod:"card" }` → `redirectUrl` should be UnifiedCheckout with card form.
   `POST ... { paymentMethod:"wallet" }` → UnifiedCheckout with wallet phone input.

## Code improvements (commit 5167292)

- `PaymobService.cs:19` inject `ILogger<PaymobService>`
- `PaymobService.cs:34` `ResolveIntegrationId` safe `TryParse` for placeholder `YOUR_...`
- `PaymobService.cs:182` logs Paymob body + surfaces `detail` so 404 is visible to client

## References

- `ClinicHub.Infrastructure/Services/Paymob/PaymobService.cs:116` `CreateIntentionAsync`
- `ClinicHub.Application/Features/AdminPayments/PaymentMethodMapper.cs:7`
- `AGENTS.md:62` — `appsettings.json` gitignored
