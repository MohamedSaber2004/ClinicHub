# README — Web Dashboards Integration (Doctor + Staff)

> **Audience:** Web frontend team (doctor dashboard & staff/reception dashboard).
> **Flow:** Patients send booking requests → staff/doctor **accept or reject** → on accept
> the backend creates the payment + Paymob checkout and notifies the patient → patient pays
> from the **mobile app only** → appointment becomes confirmed. The dashboards **never** open
> the payment page.
> Single source of truth:
> [`docs/frontend/appointment-request-payment-flow.md`](../frontend/appointment-request-payment-flow.md).
> All responses use `{ success, message, data, errors, statusCode }`, localized
> (default **Arabic**, switch with `Accept-Language: ar | en`).

---

## 1. Base & auth

| Item | Value |
|---|---|
| Base URL | `https://<api-host>/api/v1` |
| Auth | `Authorization: Bearer <accessToken>` (dashboard login) |
| Login (web) | `POST /api/v1/auth/login-web` — returns token + roles; blocks users without an active subscription |
| Language | `Accept-Language: ar` (default) or `en` |

---

## 2. Status model (source of truth = API `status` int)

| Value | Name | Meaning |
|---|---|---|
| `0` | pending (قيد الانتظار) | request submitted, not yet approved |
| `4` | reserved (محجوز مؤقتاً) | temporary slot hold — treat like pending |
| `6` | accepted / awaiting-payment (بانتظار الدفع) | **accepted → payment link sent; patient has NOT paid** |
| `1` | confirmed (مؤكد) | **payment succeeded** (webhook) — only state that allows check-in/complete |
| `3` | completed (مكتمل) | visit done |
| `2` | cancelled (ملغي) | cancelled / doctor-rejected |
| `7` | rejected (مرفوض) | staff-rejected |
| `5` | noshow (لم يحضر) | patient didn't show |

### Hard rules

- **No auto-confirmation.** A request can never become `1` without payment.
- **Paid is final.** Only `1` allows تسجيل وصول (check-in) / إكمال (complete). `6` cannot be
  checked in, completed, or re-rejected from the dashboards (patient can cancel).
- **Reject is final.** `2`/`7` can't be flipped back.
- **Accept creates the payment automatically** — the dashboard never initiates payments.

---

## 3. Staff (reception) dashboard

All staff endpoints require the `Staff` role + `ManageStaff` plan permission.

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/v1/staff/appointments?status=&date=&patientName=&pageNumber=&pageSize=` | List (day-filtered) |
| `PUT` | `/api/v1/staff/appointments/{id}/approve` | **Accept** → status `6` + payment created + link sent (§4) |
| `PUT` | `/api/v1/staff/appointments/{id}/reject` | Reject → status `7` (body `{ "reason": "..." }`) |
| `PUT` | `/api/v1/staff/appointments/{id}/check-in` | تسجيل وصول — **only when status = `1`** |
| `PUT` | `/api/v1/staff/appointments/{id}/complete` | إكمال — **only when status = `1`** |

### UI expectations (per status)

- **Status filter:** قيد الانتظار / **بانتظار الدفع** / مؤكد / ملغي / منتهي.
- `0` / `4` → **قبول + رفض** buttons.
  - قبول shows a confirm dialog: `"سيتم إرسال رابط الدفع للمريض"` → then call `approve`.
  - رفض prompts for an **optional reason** → call `reject`.
- `6` → amber badge **بانتظار الدفع**, hint `"بانتظار دفع المريض"`, **no actions**.
- `1` → **تسجيل وصول** button only (check-in). After check-in, إكمال may be shown (status `1` still allows complete).
- `2` / `7` → no actions (show reason if present).

---

## 4. Doctor dashboard

All doctor endpoints require the `Doctor` role + `ManageAppointments` plan permission.

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/v1/doctors/appointments?status=&searchTerm=&startDate=&endDate=&pageNumber=&pageSize=` | List |
| `PUT` | `/api/v1/doctors/appointments/{id}/status` | Unified: body `{ "status": 6, "notes": "" }` |
| `PUT` | `/api/v1/doctors/appointments/{id}/accept` | Dedicated accept (behaves exactly like `status = 6`) |
| `PUT` | `/api/v1/doctors/appointments/{id}/reject` | Dedicated reject (body `{ "reason": "..." }`) |
| `PUT` | `/api/v1/doctors/appointments/{id}/complete` | Dedicated complete (**only from `1`**) |

### `PUT /status` — accepted codes

| Code | Meaning | Notes |
|---|---|---|
| `6` | قبول (accept) | → creates payment + sends link (§5). Also handles legacy `1` the same way |
| `2` | رفض (reject) | body `notes` is used as the reason |
| `3` | إكمال (complete) | **only from `1`** |
| `5` | لم يحضر (no-show) | **only from `1`** |

### UI expectations (per status)

- `0` / `4` → **قبول + رفض** buttons.
  - قبول confirms and sends **status `6`** — never `1`.
- `6` → amber badge **بانتظار الدفع**, hint `"بانتظار دفع المريض"`, **no actions**.
- `1` → **إكمال** button only (and optionally لم يحضر / no-show).
- `2` / `7` → no actions.

---

## 5. Accept response (⚠️ new contract)

`approve` (staff), `accept` (doctor) and `status = 6` no longer return `data: true`.
They return the created payment envelope (and the patient gets the same link via push):

```json
{
  "success": true,
  "data": {
    "appointmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": 6,
    "paymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "amount": 300.00,
    "currency": "EGP",
    "paymobRedirectUrl": "https://accept.paymob.com/unifiedcheckout/?publicKey=...&clientSecret=...",
    "paymobPaymentKey": "paymob-payment-key"
  },
  "message": "تم قبول الحجز وتم إرسال رابط الدفع للمريض",
  "statusCode": 200
}
```

> `paymobRedirectUrl` is pushed to the patient — **the dashboards must not display or open it.**

For doctor `PUT /status` with codes `2`/`3`/`5`, `data` is `null` (success only).

---

## 6. Error codes

| Code | When | Typical message (ar) |
|---|---|---|
| `400` | Accept/reject on non-`0` appointment · check-in/complete on status ≠ `1` · no clinic fee configured | `لا يمكن الرد على هذا الموعد في حالته الحالية` / `لا توجد رسوم استشارة محددة للعيادة` |
| `401` | Missing/invalid token | — |
| `403` | Appointment belongs to another clinic/doctor | `ليس لديك صلاحية الرد على طلب الموعد هذا` |
| `404` | Appointment not found | `الموعد غير موجود` |
| `409` | Double accept (payment already exists) | `تم قبول هذا الحجز مسبقاً` |

Errors use the same envelope: `{ success: false, message, errors, statusCode }`.

---

## 7. Do's / Don'ts

- ✅ Use the API `status` int as the single source of truth — never compute it client-side.
- ✅ Show the confirm dialog "سيتم إرسال رابط الدفع للمريض" before قبول.
- ✅ Map `accepted`(6) and `awaiting-payment` → **بانتظار الدفع** badge.
- ❌ **Never** open the Paymob page from the dashboards (payment is mobile-only).
- ❌ Never send `status = 1` from قبول — the webhook sets `1` after payment.
- ❌ Don't show check-in/complete for `6` — the backend rejects it (400).
