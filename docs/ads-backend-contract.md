# Ads Backend Contract

Backend contract for clinic ads: order creation, online payment, and webhook-driven activation.

## Ad Statuses

| Value | Enum | Meaning |
|-------|------|---------|
| 0 | `PendingPayment` | معلق الدفع — created when the order is placed, awaiting Paymob confirmation |
| 1 | `Active` | نشط — set by the payment webhook only |
| 2 | `Expired` | منتهي |
| 3 | `Deactivated` | معطل |

**Only the Paymob payment webhook flips an ad from 0 → 1.** No polling, client call, or manual endpoint does this.

## Payment Flow

```
POST /api/v1/clinics/{clinicId}/ads/orders  (ClinicOwner)
  → creates Advertisement (status 0) + Payment (Processing, PaymobOrderId)
  → returns PaymobRedirectUrl + PaymobPaymentKey

Patient pays at Paymob hosted checkout
  → Paymob POSTs a webhook to /api/v1/payments/webhook
  → backend validates HMAC, marks Payment Paid
  → finds Advertisement by PaymentId
  → if status == 0 (PendingPayment): Activate() → status 1, StartDate=now, EndDate=now+DurationDays
```

## Webhook Endpoint

- **URL:** `POST /api/v1/payments/webhook` (AllowAnonymous)
- **Body (JSON):** Paymob webhook payload
  ```json
  {
    "type": "transaction",
    "hmac": "<hmac-sha512 hex>",
    "obj": { "order": { "id": 12345 }, "success": true, "...": "..." }
  }
  ```
  Note: `type` is matched case-insensitively (`"TRANSACTION"` / `"transaction"`); `hmac` and `obj` come from the **body** (not the query string).
- **Validation:** HMAC-SHA512 over the 20 documented transaction fields (amount_cents, created_at, currency, error_occured, has_parent_transaction, id, integration_id, is_3d_secure, is_auth, is_capture, is_refunded, is_standalone_payment, is_voided, order.id, owner, pending, source_data.pan, source_data.sub_type, source_data.type, success).
- **Response:** `200 true` when accepted (idempotent — repeats for an already-Paid payment are skipped and still return `200 true`).
- **Failure:** invalid/empty HMAC → `200 false`.

## Endpoints

### POST /api/v1/clinics/{clinicId}/ads/orders
Body:
```json
{ "adPackageId": "guid", "durationDays": 30, "returnUrl": "optional" }
```
Response `201`:
```json
{
  "paymentId": "guid",
  "refNumber": "PM-2026-XXXXXX",
  "amount": 500,
  "currency": "EGP",
  "status": 1,
  "paymobRedirectUrl": "https://accept.paymob.com/...",
  "paymobPaymentKey": "..."
}
```
Errors: 404 package/clinic not found, 400 inactive package / invalid duration / booking-config missing, 403 clinic not eligible (no active Advanced-Reports subscription).

### GET /api/v1/clinics/{clinicId}/ads?status=0|1|2|3
List of the clinic's ads, newest first. Each item:
```json
{
  "id": "guid",
  "packageId": "guid",
  "packageNameAr": "باقة أساسية",
  "durationDays": 30,
  "amount": 500,
  "currency": "EGP",
  "status": 0,
  "startDate": "2026-08-05T00:00:00Z",
  "endDate": "2026-09-04T00:00:00Z",
  "createdAt": "2026-08-05T09:00:00Z"
}
```

### GET /api/v1/ads/packages
Active ad packages (id, name, nameAr, price, durationDays).

## Activation Guarantees

- Webhook is **idempotent**: a duplicate callback for an already-Paid payment returns 200 without side effects.
- The ad lookup is by `PaymentId` (created together with the order), so the webhook activates exactly the ad the clinic paid for.
- If the webhook fails HMAC validation, Paymob retries; the ad stays `PendingPayment` (0) until a valid success callback arrives.
- A paid transaction whose payment type/action is unrecognized is logged (warning) — never silently lost.

## Key Files

- `ClinicHub.API/Controllers/Version1/PaymentsController.cs` — webhook endpoint
- `ClinicHub.API/Controllers/Version1/AdsController.cs` — ads endpoints
- `ClinicHub.Application/Features/Ads/AdsOrderProcessor.cs` — order creation
- `ClinicHub.Application/Features/Payment/Commands/ConfirmPaymentWebhook/ConfirmPaymentWebhookCommandHandler.cs` — webhook handling + ad activation
- `ClinicHub.Domain/Entities/Advertisement.cs` — entity + `Activate()`
