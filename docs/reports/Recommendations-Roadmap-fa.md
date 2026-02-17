# گزارش پیشنهادات و نقشه راه فنی
**پروژه:** Clean Architecture Base  
**نسخه:** 1.0  
**تاریخ:** 2026-02-17  
**نوع سند:** Actionable Roadmap (بدون ابهام برای تیم اجرا)

---

## 1) هدف سند
این سند برای برنامه‌ریزی فاز بعدی نوشته شده و مشخص می‌کند:
1. چه چیزهایی باید اضافه یا بهبود داده شود.
2. چرا هر مورد مهم است.
3. چه خروجی قابل تحویل (Definition of Done) دارد.
4. ترتیب اجرا، زمان‌بندی و اولویت چیست.

---

## 2) جمع‌بندی اجرایی
برای رسیدن به سطح **Production-Grade کامل**، 4 محور اصلی داریم:
1. تجربه توسعه‌دهنده (Developer Experience)
2. امنیت عمیق و انطباق
3. پرفورمنس و ظرفیت‌پذیری
4. ساختار ماژولار پروژه برای رشد تیمی

این نقشه راه طوری نوشته شده که تیم بتواند بدون سؤال اضافی اجرا کند.

---

## 3) بک‌لاگ پیشنهادی (گروه‌بندی‌شده)

## 3.1 Developer Experience
### DX-01 Modular DI Refactor
**مشکل:** DI مرکزی حجیم است و نگهداری را سخت می‌کند.  
**اقدام:** برای هر ماژول (`Users`, `Auth`, `Logging`, `Audit`, ...) فایل DI مستقل بساز.  
**خروجی:**
1. `AddUsersModule()`, `AddAuthModule()`, ...
2. `Program.cs` فقط orchestration سطح بالا.
**DoD:** هیچ registration ماژولی در DI عمومی پراکنده نباشد.

### DX-02 Feature Scaffolding CLI
**مشکل:** افزودن فیچر جدید زمان‌بر و ناهماهنگ است.  
**اقدام:** اسکریپت/CLI برای ساخت خودکار:
1. Endpoint
2. Command/Query
3. Validator
4. Mapping
5. Test skeleton
**DoD:** ایجاد فیچر جدید با یک دستور و کمتر از 2 دقیقه.

### DX-03 Pre-commit Quality Gate
**اقدام:** `pre-commit` با مراحل:
1. `dotnet format --verify-no-changes`
2. `dotnet build`
3. `dotnet test --no-build`
**DoD:** push بدون رعایت حداقل کیفیت ممکن نباشد.

### DX-04 Contract Tests for API Compatibility
**اقدام:** اضافه‌کردن تست compatibility برای API versionها.
**DoD:** هر breaking change در CI fail شود.

---

## 3.2 Security
### SEC-01 MFA for Privileged Roles
**اقدام:** MFA اجباری برای `admin`, `security`, `audit` roles.
**DoD:** کاربر دارای نقش حساس بدون factor دوم نتواند login کامل کند.

### SEC-02 Session & Device Management
**اقدام:** جدول session/device و endpointهای:
1. لیست sessionهای فعال
2. revoke session خاص
3. revoke all sessions
**DoD:** مدیریت نشست‌ها در سطح کاربر و ادمین قابل انجام باشد.

### SEC-03 SSRF Egress Allowlist
**اقدام:** allowlist برای outbound HTTP destinationها.
**DoD:** هر outbound خارج allowlist log + block شود.

### SEC-04 Data Classification & PII Masking Policy
**اقدام:** تعریف کلاس‌بندی داده (`Public/Internal/Confidential/Restricted`) و mask سراسری PII در log/response.
**DoD:** همه فیلدهای حساس tag شده و در لاگ به‌صورت mask ذخیره شوند.

### SEC-05 Security Headers Policy as Code
**اقدام:** policy قابل تست برای headerها + integration test.
**DoD:** نبود headerهای حیاتی در CI fail شود.

### SEC-06 Threat Modeling Cycle
**اقدام:** چرخه فصلی threat modeling با خروجی action items.
**DoD:** حداقل یک گزارش TM در هر فصل + closure tracking.

---

## 3.3 Performance & Scalability
### PERF-01 Slow Query Observatory
**اقدام:** instrumentation برای queryهای کند + داشبورد.
**DoD:** top slow queries با threshold قابل مشاهده و alertable باشد.

### PERF-02 Read Model/Projection for Hot Endpoints
**اقدام:** برای endpointهای پرترافیک projection اختصاصی بساز.
**DoD:** latency endpointهای هدف حداقل 30% کاهش یابد.

### PERF-03 Cache Strategy Matrix
**اقدام:** جدول رسمی cache برای هر endpoint:
1. key design
2. TTL
3. invalidation trigger
4. consistency mode
**DoD:** همه endpointهای read-heavy استراتژی کش مکتوب داشته باشند.

### PERF-04 Load Tests in CI (Critical Paths)
**اقدام:** اجرای k6 برای مسیرهای حیاتی در CI با threshold قطعی.
**DoD:** breach threshold باعث fail pipeline شود.

### PERF-05 Autoscaling by SLO Signals
**اقدام:** HPA/KEDA با سیگنال:
1. CPU/Memory
2. Queue depth
3. p95 latency
**DoD:** در بار بالا degraded طولانی‌مدت رخ ندهد.

---

## 3.4 Project Structure (پیشنهاد کلیدی برای ساختار)
### ARCH-01 Vertical Slice + Bounded Modules
**اقدام اصلی:**
1. ایجاد `src/Modules/<Feature>`
2. قرار دادن `Domain/Application/Infrastructure/Api` در همان feature
3. حفظ `SharedKernel` برای قراردادهای مشترک واقعی
4. تعریف `Module Composition Root` برای هر ماژول

**دلیل:**
1. افزایش cohesion در feature
2. کاهش coupling بین featureها
3. onboarding سریع‌تر تیم جدید
4. تغییرات ایزوله و قابل تست‌تر

**DoD:**
1. حداقل 2 ماژول فعلی به ساختار جدید مهاجرت کنند (`Users`, `Authorization`).
2. NetArchTest enforcement برای مرز ماژول‌ها فعال شود.

---

## 4) نقشه راه زمان‌بندی‌شده (30/60/90)

## فاز 30 روز (اولویت امنیت + DX)
1. DX-01 Modular DI
2. DX-03 Pre-commit Gate
3. SEC-01 MFA (طراحی + MVP)
4. SEC-02 Session management (schema + APIs)
5. SEC-05 Security header tests

**خروجی فاز:**
1. امنیت login ارتقا یافته
2. کیفیت توسعه یکنواخت
3. DI قابل نگهداری‌تر

## فاز 60 روز (پرفورمنس + معماری)
1. ARCH-01 Vertical Slice migration (Users/Authorization)
2. PERF-01 Slow query observability
3. PERF-03 Cache strategy matrix
4. PERF-04 CI load test critical paths
5. SEC-03 SSRF allowlist

**خروجی فاز:**
1. latency بهتر
2. معماری مقیاس‌پذیرتر
3. ریسک outbound abuse کاهش‌یافته

## فاز 90 روز (عملیات پیشرفته)
1. PERF-05 Autoscaling by SLO
2. SEC-04 Data classification + masking
3. SEC-06 Threat modeling cycle
4. governance تکمیلی برای release/incident

**خروجی فاز:**
1. عملیات production با بلوغ بالاتر
2. امنیت و انطباق پایدارتر

---

## 5) ماتریس اولویت (Impact vs Effort)
| کد | تاثیر | هزینه | اولویت |
|---|---|---|---|
| SEC-01 | خیلی بالا | متوسط | P0 |
| SEC-02 | خیلی بالا | متوسط | P0 |
| DX-01 | بالا | متوسط | P0 |
| ARCH-01 | خیلی بالا | بالا | P1 |
| PERF-01 | بالا | متوسط | P1 |
| PERF-03 | بالا | کم | P1 |
| SEC-03 | بالا | متوسط | P1 |
| PERF-05 | بالا | بالا | P2 |
| SEC-04 | بالا | بالا | P2 |
| SEC-06 | متوسط | متوسط | P2 |

---

## 6) معیار پذیرش فنی (Engineering Acceptance Criteria)
1. هر آیتم باید PRD فنی کوتاه + ADR داشته باشد.
2. هر آیتم باید test + observability + security logging داشته باشد.
3. هر آیتم schema change باید migration + rollback path داشته باشد.
4. هر آیتم production-impact باید runbook و dashboard update داشته باشد.

---

## 7) خروجی‌های مورد انتظار برای مدیریت
1. کاهش ریسک امنیتی قابل اندازه‌گیری (Auth incidents, lockouts, denied abuse).
2. بهبود شاخص‌های پرفورمنس (p95 latency, error rate).
3. کاهش زمان توسعه فیچر جدید (Lead Time).
4. کاهش زمان بازیابی سرویس (MTTR).
5. افزایش پیش‌بینی‌پذیری انتشار (Deployment Success Rate).

---

## 8) KPIs پیشنهادی برای پایش نقشه راه
1. Deployment success rate: `>= 98%`
2. Change failure rate: `<= 5%`
3. p95 latency critical endpoints: `< 500ms`
4. Mean lead time feature delivery: `-30%`
5. Security incident count per month: روند نزولی مستمر
6. MTTR: `< 30m`

---

## 9) ریسک‌های اجرایی نقشه راه و کنترل
1. ریسک scope creep
اقدام: freeze scope per sprint + change control.
2. ریسک کمبود ظرفیت تیم
اقدام: rollout phased + parallel tracks محدود.
3. ریسک شکست migration ساختاری
اقدام: feature flag + shadow rollout + rollback plan.

---

## 10) دستور اجرای بدون سؤال (Default Execution Policy)
1. هر آیتم P0 باید حداکثر ظرف 2 sprint deliver شود.
2. merge هر آیتم فقط با سبز بودن CI/Security مجاز است.
3. release آیتم‌های معماری فقط با smoke + rollback validated.
4. هر تصمیم معماری خارج از این سند باید ADR جدید بگیرد.

---

## 11) جمع‌بندی نهایی
این نقشه راه به‌صورت عملیاتی و بدون ابهام نوشته شده است.  
اگر تیم دقیقاً همین ترتیب را اجرا کند، زیرساخت از «نزدیک Production» به «Production-Grade پایدار و مقیاس‌پذیر» می‌رسد.
