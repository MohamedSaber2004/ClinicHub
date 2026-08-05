# ⏰ Hangfire — ملخص ما تم تنفيذه والخطوات القادمة

> مستند يشرح (بالعربية) ما تم بناؤه حول **Hangfire** للمهام المجدولة الزمنية في ClinicHub، وما هي الخطوات المقترحة لإكمال التكامل.

---

## 1️⃣ ما هو Hangfire ولماذا؟

Hangfire هو مكتبة مهام خلفية (Background Jobs) تعمل داخل نفس تطبيق الـ API:

| الميزة | الفائدة |
|---|---|
| تنفيذ مهمة في **وقت محدد بالضبط** (Scheduled Job) | مثل: انتهاء صلاحية إعلان في `EndDate` بالضبط |
| مهام متكررة (Recurring Job) | مثل: تنظيف دوري كل ساعة |
| **الحفظ في قاعدة البيانات** | المهام لا تضيع عند إعادة تشغيل السيرفر |
| لوحة تحكم (Dashboard) | مراقبة المهام وفشلها وإعادة تشغيلها يدويًا |

قبل Hangfire كان يوجد `BackgroundService` واحد يستخدم `Task.Delay` (فحص الاشتراكات كل ساعة) — تم حذفه واستبداله بآلية Hangfire الموحدة.

---

## 2️⃣ ما تم إعداده (الإعداد الأساسي)

| الخطوة | التفاصيل |
|---|---|
| الحزم | `Hangfire.AspNetCore` + `Hangfire.SqlServer` (في مشروعي `API` و `Infrastructure`) |
| التخزين | قاعدة **مستقلة** باسم `db47837_hangfire` (`HangfireDb` في الإعدادات) — لو غير متوفرة يرجع تلقائيًا لقاعدة `CareClinicHubDb` |
| السيرفر | `AddHangfireServer` — عاملان (`WorkerCount = 2`) |
| اللوحة | `/hangfire` — في Development مفتوحة للطلبات المحلية فقط (`LocalRequestsOnlyAuthorizationFilter`)، وفي الإنتاج بكود **SuperAdmin** فقط |
| التسجيل | `UseColouredConsoleLogProvider` + كل الجوبات تسجّل عبر `ILogger` (Serilog) |
| مكان الكود | `ClinicHub.Infrastructure/Services/BackgroundJobs/` |
| فحص الصحة | `/health` يتضمن الآن `Hangfire` health check (`AddHangfire` في `DependencyInjection.cs` بمشروع Application) |

**ملفان مهمان:**
- `ClinicHub.Application/Common/Interfaces/IBackgroundJobScheduler.cs` — واجهة نظيفة يستخدمها الـ Handlers (بدون ربط مباشر بـ Hangfire)
- `ClinicHub.Infrastructure/Services/BackgroundJobs/BackgroundJobScheduler.cs` — التنفيذ الفعلي (`BackgroundJob.Schedule<Job>...`)

---

## 3️⃣ المهام المنفذة (9 Jobs)

### أ. جوبات مُجدولة بدقة (Scheduled عند إنشاء الكيان)

| الجوب | الملف | متى تُجدول؟ | ماذا تفعل؟ |
|---|---|---|---|
| انتهاء **الإعلان** | `AdExpirationJob.MarkExpiredAsync` | عند تفعيل الإعلان (Webhook الدفع أو الدفع اليدوي) في `EndDate` | يحوّل الحالة `Active → Expired` |
| انتهاء **الاشتراك** | `SubscriptionExpirationJob.ExpireAsync` | عند إنشاء الاشتراك (3 أماكن: Webhook + إنشاء السوبر أدمن + الإنشاء اليدوي) | يحوّل `Active → Expired` |
| انتهاء **صلاحية الحجز** (Reservation TTL) | `ReservationExpirationJob.ExpireReservationAsync` | عند حجز موعد في عيادة برسوم كشف > 0 (`ExpiresAt`) | يحوّل `Reserved → Cancelled` لو ما اتدفعش خلال المهلة + **يرسل FCM للمريض** بسبب الإلغاء التلقائي |
| **إغلاق نافذة الإلغاء** | `CancellationWindowJob.CloseCancellationWindowAsync` | عند تأكيد الدفع (Webhook أو `VerifyBookingPayment`) في `PaidAt + CancellationWindowMinutes` | يرسل إشعار للمريض أن مهلة الإلغاء/الاسترداد انتهت |
| **عدم الحضور (NoShow)** | `NoShowJob.MarkNoShowAsync` | عند تأكيد الدفع/الحجز + بعد نهاية الموعد بـ 30 دقيقة | يحوّل الحجوزات `Accepted/Confirmed` غير الحاضرة إلى `NoShow` |
| **استرداد تلقائي** | `RefundRetryJob.ExecuteAsync` | عند فشل الاسترداد اللحظي في `CancelAppointment` | يُعيد محاولة الاسترداد مع تأخير تصاعدي (2^محاولة ساعة) حتى 4 محاولات |

### ب. جوبات دورية (Recurring — شبكة أمان)

| الجوب | الملف | التكرار | ماذا تفعل؟ |
|---|---|---|---|
| مسح الاشتراكات المنتهية | `SubscriptionExpirationJob.SweepExpiredAsync` | كل ساعة | تصحيح أي اشتراك فاتته الجوبة المجدولة |
| مسح الإعلانات المنتهية | `AdExpirationJob.SweepExpiredAsync` | كل ساعة | نفس الفكرة للإعلانات |
| مسح الحجوزات المنتهية | `ReservationExpirationJob.SweepExpiredReservationsAsync` | كل ساعة | تصحيح أي حجز فاتته الجوبة المجدولة + FCM للمريض |
| **الدفعات المتروكة** | `AbandonedPaymentJob.SweepAsync` | كل ساعة | أي دفع `Pending/Processing` أقدم من 24 ساعة → `Failed` + إلغاء الحجز المرتبط |
| **تنظيف التوكنات** | `TokenCleanupJob.CleanupAsync` | يوميًا | إلغاء توكنات الـ Refresh منتهية الصلاحية + مسح رموز إعادة التعيين/التحقق منتهية الصلاحية |
| **تذكيرات انتهاء الصلاحية** | `ExpiryReminderJob.SendRemindersAsync` | يوميًا | FCM لمالك العيادة قبل **3 أيام** و **يوم واحد** من انتهاء الاشتراك أو الإعلان |

---

## 4️⃣ الأماكن التي تم ربطها (Wiring)

| الملف | التغيير |
|---|---|
| `CreateAppointmentCommandHandler` | حجز الموعد (`Reserve`) عند وجود رسوم كشف + جدولة انتهاء الحجز |
| `ConfirmPaymentWebhookCommandHandler` | جدولة انتهاء الإعلان + انتهاء الاشتراك + إغلاق نافذة الإلغاء + جدولة `NoShow` |
| `CreateManualPaymentCommandHandler` | جدولة انتهاء الإعلان (الدفع النقدي) |
| `AdminCreateSubscriptionCommandHandler` + `CreateSubscriptionCommandHandler` | جدولة انتهاء الاشتراك |
| `VerifyBookingPaymentCommandHandler` | تأكيد الحجز + جدولة إغلاق نافذة الإلغاء + جدولة `NoShow` |
| `CancelAppointmentCommandHandler` | مهلة الإلغاء تُحسب من **وقت الدفع** (`PaidAt`) وليس وقت الإنشاء؛ إن فشل الاسترداد اللحظي مع Paymob يتم **جدولة إعادة محاولة** بدل رفض العملية |
| `AppointmentAcceptanceService` | عند قبول العيادة للحجز → جدولة `NoShow` بعد نهاية الموعد + 30 دقيقة |
| `InitiateBookingPaymentCommandHandler` | إنشاء **Paymob Checkout حقيقي** (Order + Redirect URL) بدل الدفع `cash` الوهمي |
| `Program.cs` | لوحة `/hangfire` + تسجيل الجوبات الدورية (بما فيها `expiry-reminders`) |

---

## 5️⃣ كيف تجرّبها محليًا؟

1. شغّل التطبيق — ستظهر جداول `HangFire.*` في قاعدة البيانات تلقائيًا.
2. افتح `http://localhost:{port}/hangfire` (مفتوحة في Development).
3. من اللوحة يمكنك: مشاهدة المهام المجدولة، تشغيلها يدويًا، ومعرفة أي فشل.

---

## 6️⃣ الخطوات القادمة المقترحة (Next Steps)

### عاجل / مهم
- [x] **ربط دفع الحجز بـ Paymob فعليًا** — `InitiateBookingPayment` ينشئ الآن Checkout حقيقي (Order + Redirect URL) — يُرجع `RedirectUrl` للعميل ليكمل الدفع.
- [x] **تذكيرات انتهاء الاشتراك/الإعلان** — جوبة يومية (`ExpiryReminderJob`) ترسل FCM للمالك قبل 3 أيام + يوم.
- [x] **حماية لوحة `/hangfire`** — Development: طلبات محلية فقط؛ Production: SuperAdmin فقط.
- [ ] **فحص دفع الحجز end-to-end مع Paymob** — تجربة فعلية: حجز → Checkout Paymob → Webhook → تأكيد + جدولة النوافذ.

### تحسينات
- [x] **إشعار إلغاء الحجز التلقائي** — `ReservationExpirationJob` يرسل FCM للمريض عند انتهاء صلاحية الحجز.
- [x] **`NoShow` التلقائي** — `NoShowJob` بعد نهاية الموعد + 30 دقيقة.
- [x] **استرداد تلقائي للدفعات المتروكة** — `RefundRetryJob` بمحاولات متصاعدة حتى 4 مرات.
- [ ] **إلغاء الجوبات المجدولة عند الإلغاء اليدوي** — مثلاً لو أدمن عطّل الإعلان يدويًا قبل `EndDate`، يمكن حذف الجوبة المجدولة (`BackgroundJob.Delete(jobId)`) أو تركها (آمنة لأنها تفحص الحالة).
- [ ] **Retention / تنظيف** — حذف الإشعارات والسجلات القديمة أسبوعيًا + توكنات الـ Refresh المحذوفة نهائيًا.

### مراقبة
- [x] إضافة `Hangfire` لفحص الصحة (`HealthChecks`) — يظهر في `/health`.
- [ ] ربط سجلات الفشل بـ **Seq** (تسجيل `Job` سجل-فشل عند أي استثناء).

---

## 7️⃣ ملاحظات تقنية مهمة

- كل الجوبات **آمنة ضد التكرار** (تفحص الحالة قبل أي تغيير) — تشغيلها مرتين لا يسبب مشاكل.
- الجوبات المجدولة تُحفظ في قاعدة البيانات → **تبقى حتى بعد إعادة تشغيل السيرفر** (لكن الفشل المتكرر يحتاج متابعة من اللوحة).
- **لا يوجد** `Hangfire.Serilog` على NuGet — استخدمنا الـ Colored Console + `ILogger` داخل الجوبات.
- الحزم المثبتة: `Hangfire.AspNetCore 1.8.24`، `Hangfire.SqlServer 1.8.24`، `Hangfire.Core 1.8.24` (في `API` و `Infrastructure`) + `AspNetCore.HealthChecks.Hangfire 9.0.0` (في `Application`).
- أنواع الإشعارات الجديدة المضافة لـ `NotificationType`: `CancellationWindowClosed=6`، `SubscriptionExpiring=7`، `RefundProcessed=8`، `AdExpiring=9` (نصوصها العربية في `NotificationBuilderService`).
