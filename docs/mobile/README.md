# README — Mobile App (Patient Side) Integration

> **Audience:** Mobile team (patient app).
> **Flow:** Book an appointment request → wait for clinic approval → pay (Paymob hosted
> page) → appointment confirmed. This README is the mobile companion to
> [`docs/frontend/appointment-request-payment-flow.md`](../frontend/appointment-request-payment-flow.md)
> (the single source of truth). All responses use the standard
> `{ success, message, data, errors, statusCode }` envelope and are localized
> (default **Arabic**, switch via the `Accept-Language` header: `ar` / `en`).

---

## 1. Base & auth

| Item | Value |
|---|---|
| Base URL | `https://<api-host>/api/v1` (versioned) |
| Auth | `Authorization: Bearer <accessToken>` for all patient endpoints |
| Language | `Accept-Language: ar` (default) or `en` |
| Login | `POST /api/v1/auth/login` (`{ email, password }`) |
| Signup | `POST /api/v1/auth/signup` |
| Refresh | `POST /api/v1/auth/refresh-token` |
| Profile | `GET /api/v1/auth/profile` |

---

## 2. Lifecycle (status field is an int)

| Value | Name | Patient sees | Patient action |
|---|---|---|---|
| `0` | pending (قيد الانتظار) | قيد الانتظار | none (إلغاء optional) |
| `4` | reserved (محجوز مؤقتاً) | قيد الانتظار | wait (temporary slot hold) |
| `6` | accepted / awaiting-payment (بانتظار الدفع) | بانتظار الدفع | **ادفع الآن** |
| `1` | confirmed (مؤكد) | مؤكد | view details |
| `3` | completed (مكتمل) | مكتمل | — |
| `2` | cancelled (ملغي) | ملغي | show `rejectionReason` |
| `7` | rejected (مرفوض) | مرفوض | show `rejectionReason` |
| `5` | noshow (لم يحضر) | لم يحضر | — |

**Rules the app must respect**

- A booking **never** becomes confirmed without payment. `1` is only ever set
  server-side by the Paymob webhook — never by the client.
- Status `6` (accepted, unpaid) is the only state where the "ادفع الآن" button appears.
- After acceptance the payment link is sent to the patient — the dashboards do **not**
  open the payment page, the mobile app does.

---

## 3. Flow

### 3.1 Book a request

```http
GET /api/v1/clinics/{clinicId}/doctors/{doctorId}/slots?date=2026-08-05
```

Pick a slot, then:

```http
POST /api/v1/appointments
Content-Type: application/json

{
  "doctorId": "22222222-...",
  "clinicId": "11111111-...",
  "appointmentDate": "2026-08-05",
  "startTime": "10:00",
  "endTime": "10:30",
  "appointmentType": 0,
  "patientFullName": "أحمد محمد",
  "patientAge": 30,
  "patientGender": 0,
  "complaint": "صداع",
  "chronicDiseases": null
}
```

The appointment is created with `status = 0` (may temporarily be `4` reserved while the
slot is held). Response: 201 with the appointment payload.

### 3.2 Acceptance push notification

When staff/doctor accepts, the app receives a **push notification**:

```json
{
  "title": "تم قبول حجزك",
  "body": "أكمل الدفع لتأكيد موعدك في مجمع عيادات السلام الطبي بتاريخ 2026-08-05",
  "data": {
    "type": "AppointmentConfirmation",
    "appointmentId": "3fa85f64-...",
    "clinicName": "مجمع عيادات السلام الطبي",
    "date": "2026-08-05",
    "paymentUrl": "https://accept.paymob.com/unifiedcheckout/?publicKey=...&clientSecret=...",
    "link": "https://<app-deep-link>/appointments/3fa85f64-..."
  }
}
```

- Use `data.type` to route (`AppointmentConfirmation`).
- `data.paymentUrl` → open the **pay screen** (hosted Paymob checkout).
- `data.link` → app deep link to the appointment details screen.

### 3.3 My appointments (list + payment info)

```http
GET /api/v1/appointments/my?status=6
```

`status` query is optional (any int from the table above). Response:

```json
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "clinicId": "11111111-1111-1111-1111-111111111111",
      "clinicName": "مجمع عيادات السلام الطبي",
      "doctorId": "22222222-2222-2222-2222-222222222222",
      "doctorName": "د. أحمد محمد",
      "date": "2026-08-05",
      "startTime": "10:00",
      "endTime": "10:30",
      "status": 6,
      "rejectionReason": null,
      "payment": {
        "paymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "amount": 300.00,
        "currency": "EGP",
        "paymentStatus": 0,
        "paymobRedirectUrl": "https://accept.paymob.com/unifiedcheckout/?publicKey=...&clientSecret=..."
      }
    }
  ],
  "message": "Success",
  "statusCode": 200
}
```

Contract notes:

- `payment` is present only when a payment record exists.
- `payment.paymobRedirectUrl` is present **only while `status = 6`** (accepted & unpaid).
  This is the hosted checkout page — open it in a WebView/browser to pay.
- `payment.paymentStatus`: `0` معلق / `1` ناجح / `2` فاشل / `3` مسترد / `4` قيد المعالجة.

### 3.4 Pay

1. Show **"ادفع الآن"** only when `status == 6`.
2. Open `payment.paymobRedirectUrl` (unified checkout hosted page).
3. Patient completes payment on the Paymob page.
4. Paymob webhook confirms server-side → `status` flips to `1` (مؤكد) automatically.
5. Poll `GET /api/v1/payments/status/{appointmentId}` or refresh
   `GET /api/v1/appointments/my` until `status == 1`.
6. If the payment attempt failed, the request stays `6` — the patient can retry the
   same link (or a fresh one appears after re-acceptance).

### 3.5 Cancel (optional)

```http
PUT /api/v1/appointments/{id}/cancel
Content-Type: application/json

{ "cancellationReason": "غير قادر على الحضور" }
```

- Allowed from `0`, `4`, `6` (and `1` — triggers a refund of the paid amount).
- Not allowed from `2`, `3`, `7`, `5`.

---

## 4. UI status labels (reference)

| status | Label (ar) | Action shown |
|---|---|---|
| `0` / `4` | قيد الانتظار | none (إلغاء optional) |
| `6` | بانتظار الدفع | **ادفع الآن** |
| `1` | مؤكد | details |
| `3` | مكتمل | — |
| `2` | ملغي | show `rejectionReason` |
| `7` | مرفوض | show `rejectionReason` |
| `5` | لم يحضر | — |

---

## 5. Error codes

| Code | When | Typical message (ar) |
|---|---|---|
| `400` | Booking/validation error, cancel in wrong status, no fee configured | `لا يمكن إلغاء هذا الموعد في حالته الحالية` / `لا توجد رسوم استشارة محددة للعيادة` |
| `401` | Missing/invalid/expired token | — |
| `403` | Not your appointment | `ليس لديك صلاحية إلغاء هذا الموعد` |
| `404` | Not found | `الموعد غير موجود` |
| `409` | Double accept attempted server-side | `تم قبول هذا الحجز مسبقاً` |

Errors come in the same envelope: `{ success: false, message, errors, statusCode }`.

---

## 6. Do's / Don'ts

- ✅ Treat `status` as the single source of truth — never compute it client-side.
- ✅ Open `paymobRedirectUrl` in the system browser/WebView (never an embedded
  in-app "payment" implementation).
- ✅ Handle the push `data.paymentUrl` to deep-link straight into the pay screen.
- ❌ Never set an appointment to confirmed from the app.
- ❌ Don't show "ادفع الآن" for anything except `status == 6`.
- ❌ Don't implement Paymob server calls — the backend creates the checkout and
  receives the webhook; the app only opens the hosted page.
