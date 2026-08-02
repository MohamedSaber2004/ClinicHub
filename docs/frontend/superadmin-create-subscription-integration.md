# Superadmin "إضافة اشتراك" (Add Subscription) — Frontend Integration Guide

> **Audience:** Frontend team (superadmin dashboard).
> **Status:** ✅ **Implemented and ready** — `POST /api/v1/admin/dashboard/subscriptions`
> is deployed on the backend. The Subscription Management page
> (`/Admin/SubscriptionManagement`) can now call it for the **"إضافة اشتراك"** modal.

---

## 1. Endpoint at a glance

| Method | Route | Purpose |
|--------|-------|---------|
| `POST` | `/api/v1/admin/dashboard/subscriptions` | Create an **active** subscription for a clinic (admin-initiated, no Paymob) |

- **Auth:** `Authorization: Bearer <token>` — requires the **SuperAdmin** role, otherwise `401`.
- **Language:** `Accept-Language: ar` (default) — all messages come localized (Arabic).
- **Response wrapper:** every response is `ApiResponse<T>`:

```json
{ "success": true, "data": { }, "message": "string", "errors": [], "statusCode": 201 }
```

---

## 2. Request body

```json
{
  "clinicId": "00000001-0000-0000-0000-000000000001",
  "planId": "b1f6c1a0-1111-1111-1111-111111111111",
  "period": 0,
  "startDate": "2026-08-02"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `clinicId` | Guid | ✅ | Clinic that owns the subscription |
| `planId` | Guid | ✅ | Plan being granted |
| `period` | int | ✅ | `0` = monthly (شهري), `1` = yearly (سنوي) |
| `startDate` | date | ❌ | `YYYY-MM-DD`. Defaults to **today** when omitted |
| `amount` | decimal | ❌ | **Do not send it** — backend defaults to the plan price (`PriceMonthly` / `PriceYearly` per `period`) |

> ⚠️ The frontend sends `clinicId`, `planId`, `period`, `startDate` only. Never send `amount`.

---

## 3. Success response — `201 Created`

```json
{
  "success": true,
  "data": {
    "id": "a2c3d4e5-...-guid",
    "clinicId": "00000001-0000-0000-0000-000000000001",
    "clinicName": "مجمع عيادات السلام الطبي",
    "planId": "b1f6c1a0-...-guid",
    "planName": "Advanced",
    "period": 0,
    "startDate": "2026-08-02T00:00:00",
    "endDate": "2026-09-02T00:00:00",
    "status": 0,
    "amount": 500.00,
    "paidAt": "2026-08-02T10:00:00",
    "isActive": true,
    "permissions": ["ManageAppointments", "PatientRecords", "..."]
  },
  "message": "تم إنشاء الاشتراك بنجاح",
  "errors": [],
  "statusCode": 201
}
```

`data` is the **same `SubscriptionDto` shape** returned by `GET /api/v1/admin/dashboard/subscriptions`
(the list endpoint) — you can append it to the table rows directly or just re-fetch the list.

---

## 4. What the backend does (business behavior — important!)

Creating a subscription via this endpoint behaves **exactly like a successful Paymob checkout**:

1. **Creates/updates an ACTIVE subscription** (`status: 0`, `isActive: true`), `endDate` computed from `period`.
2. **Unlocks the clinic owner dashboard immediately** — `GET /api/v1/subscriptions/my` returns it
   (the clinic dashboard checks this on every page load), so all features open without any clinic-side action.
3. **Appears in the admin list** — `GET /api/v1/admin/dashboard/subscriptions` returns it
   (refresh the table after creation).
4. **Creates a `Subscription` payment record** automatically (method `cash`, marked paid) —
   it shows on the Payments page under **إيرادات الاشتراكات**. The admin does **not** need to
   record it separately via `POST /api/v1/admin/payments/manual`.

### Duplicate handling — the rule the frontend must know

| Case | Backend behavior |
|------|------------------|
| Clinic **still has a valid active subscription** (`status: 0` **and** `endDate` in the future) — same **or different** plan | **Rejects with `400`**: «يوجد اشتراك نشط لهذه العيادة بالفعل». No data is changed, no payment is created. |
| Clinic has an active record that **already expired by date** (`endDate` in the past) | Backend auto-marks it `Expired` and creates the new subscription normally. |
| Clinic only has cancelled / revoked / expired subscriptions | Creates a new active subscription normally. |
| Clinic has no subscriptions at all | Creates a new active subscription normally. |

> The UI does **not** need to pre-check — the backend decides. The typical flow is: open the modal,
> select the clinic, and the backend tells you if the clinic is already subscribed. Toast the
> `message` («يوجد اشتراك نشط لهذه العيادة بالفعل») so the admin understands why it was rejected.
> You may optionally disable the submit button while a successful/error response is pending, but
> **do not** silently succeed — the modal must stay open on `400` and show the message.

---

## 5. Error handling (what the modal must do)

| HTTP | Meaning | Frontend action |
|------|---------|-----------------|
| `400` | Validation error **or** business rejection | If `errors` is non-empty, toast the first value (e.g. `errors.startDate[0]`). If `errors` is empty, toast `message` — this is the already-subscribed case («يوجد اشتراك نشط لهذه العيادة بالفعل»). Keep the modal open on `400`. |
| `401` | Missing/invalid/expired admin token | Redirect to login |
| `403` | Not SuperAdmin | Toast `message` |
| `404` | Clinic or plan not found / plan inactive | Toast `message` (e.g. «العيادة غير موجودة», «هذه الباقة غير نشطة») |

Error envelope example:

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": { "StartDate": ["Start date cannot be in the past"] },
  "statusCode": 400
}
```

Validation rules enforced by the backend:

- `clinicId` — required (must exist) → else `404`
- `planId` — required (must exist **and be active**) → else `404`
- `period` — must be `0` or `1`
- `startDate` — optional; **must not be before today** → `400`
- `amount` — optional; if provided must be `> 0`

---

## 6. Wiring the "إضافة اشتراك" modal (step by step)

The modal already exists on `/Admin/SubscriptionManagement`. Wire it like this:

### 6.1 Load the dropdowns

| Dropdown | Endpoint | Response |
|----------|----------|----------|
| العيادة (Clinic) | `GET /api/v1/admin/dashboard/clinics` | `data` = list of clinics (`id`, `name`, ...) |
| الباقة (Plan) | `GET /api/v1/admin/plans` | `data` = list of plans (`id`, `name`/`nameAr`, `priceMonthly`, `priceYearly`, `isActive`) |

Load both when the modal opens (or on page load) and populate the selects. Only active plans
should be selectable — filter by `isActive === true`.

### 6.2 Form fields

- **Clinic** — select (required)
- **Plan** — select (required)
- **Period** — segmented control: شهري `0` / سنوي `1` (required, default `0`)
- **Start date** — date input (optional, default today). Disable past dates in the picker.
- Optionally display the computed price (plan `priceMonthly` / `priceYearly` per period) as a
  preview label — it's informational only, **not sent** in the request.

### 6.3 Submit

```js
async function createSubscription(payload) {
  const res = await fetch('/api/v1/admin/dashboard/subscriptions', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
      'Accept-Language': 'ar'
    },
    body: JSON.stringify(payload)
  });
  const json = await res.json();

  if (!res.ok) {
    const firstError = json.errors && Object.values(json.errors).flat()[0];
    showToast(firstError || json.message || 'حدث خطأ غير متوقع', 'error');
    if (res.status === 401) redirectToLogin();
    return null;
  }

  showToast(json.message || 'تم إنشاء الاشتراك بنجاح', 'success');
  return json.data; // SubscriptionDto — can be prepended to the table
}
```

Payload built from the form:

```js
{
  clinicId: form.clinicId,
  planId: form.planId,
  period: form.period,        // 0 | 1
  startDate: form.startDate   // 'YYYY-MM-DD' or null to default to today
}
```

### 6.4 After success

1. Close the modal + reset the form.
2. Refresh the table — call `GET /api/v1/admin/dashboard/subscriptions` (keep the current
   filters/pagination) **or** prepend `json.data` if the current filter matches `status: 0`.
3. No other page needs refreshing — the clinic owner side updates automatically server-side.

---

## 7. Manual verification (curl)

```bash
# Expect 401 when unauthenticated (route exists) — NOT 404
curl -s -o /dev/null -w "%{http_code}\n" \
  -X POST https://doctory-icare.runasp.net/api/v1/admin/dashboard/subscriptions

# With a SuperAdmin token — expect 201 + subscription data
curl -s -X POST \
  -H "Authorization: Bearer <SUPERADMIN_TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{ "clinicId": "<CLINIC_ID>", "planId": "<PLAN_ID>", "period": 0 }' \
  https://doctory-icare.runasp.net/api/v1/admin/dashboard/subscriptions
```

Then log into the clinic owner dashboard — all pages must open (no redirect to the renewal page),
proving the subscription is active and synced. A paid `Subscription` payment must also appear on
the Payments page.
