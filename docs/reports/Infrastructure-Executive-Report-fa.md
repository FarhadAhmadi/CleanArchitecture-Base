# گزارش جامع فنی زیرساخت (مرجع واحد اجرا)
**پروژه:** Clean Architecture Base  
**نسخه سند:** 3.0  
**تاریخ:** 2026-02-17  
**وضعیت:** مرجع رسمی برای توسعه، عملیات، امنیت و ارائه مدیریتی

---

## 1) هدف این سند
این فایل «مرجع واحد» است تا تیم بدون ابهام بتواند:
1. وضعیت فعلی سیستم را بفهمد.
2. معماری و مرز ماژول‌ها را دقیق رعایت کند.
3. سیستم را Local / Staging / Production راه‌اندازی کند.
4. امنیت، پرفورمنس و عملیات را استاندارد اجرا کند.
5. backlog بهبود را بدون سؤال اضافه پیاده‌سازی کند.

---

## 2) خلاصه مدیریتی
این پروژه از یک API پایه به یک زیرساخت نزدیک Production ارتقا یافته است و اکنون پوشش می‌دهد:
1. امنیت هویتی و دسترسی (`JWT`, `OAuth2`, lockout, RBAC/PBAC).
2. قابلیت اطمینان داده و رویداد (`Outbox/Inbox`, `RabbitMQ`, retry).
3. لاگینگ و ممیزی قابل استناد (checksum integrity + audit chain).
4. عملیات و مشاهده‌پذیری (`OpenTelemetry`, SLO metrics, alert worker).
5. تحویل امن (`CI/CD`, security scan, blue/green, rollback).
6. زیرساخت قابل تکرار (`Terraform`, DR scripts, performance automation).

---

## 3) نقشه کلان معماری
### 3.1 لایه‌ها
1. `Domain`: قوانین کسب‌وکار، مدل‌های هسته، بدون وابستگی به زیرساخت.
2. `Application`: use-caseها، CQRS، قراردادهای abstraction.
3. `Infrastructure`: دیتابیس، امنیت، پیام، کش، لاگ، مانیتورینگ.
4. `Web.Api`: transport/API، middlewareها، endpoint orchestration.
5. `DevOps/Platform`: CI/CD, IaC, DR, release strategy, performance validation.

### 3.2 اصول معماری
1. `High Cohesion`: هر ماژول یک مسئولیت مرکزی دارد.
2. `Low Coupling`: وابستگی‌ها با interface و قرارداد کنترل می‌شوند.
3. `Dependency Rule`: لایه‌های داخلی مستقل از جزئیات بیرونی هستند.
4. `Operational by Design`: هر قابلیت همراه logging/monitoring/security controls.

---

## 4) دسته‌بندی ماژول‌ها (بر اساس High Cohesion / Low Coupling)
### 4.1 Domain Modules
1. `Domain.Users`: مدل کاربر، lockout fields.
2. `Domain.Authorization`: نقش/مجوز و permission codes.
3. `Domain.Todos`: موجودیت Todo.
4. `Domain.Logging`: رخداد لاگ و integrity flags.
5. `Domain.Auditing`: مدل audit tamper-evident.

### 4.2 Application Modules
1. `Application.Users`: Register/Login/Refresh/Revoke/Get.
2. `Application.Authorization`: assign role/permission + access queries.
3. `Application.Todos`: CRUD و queryهای todo.
4. `Application.Abstractions.*`: Data/Auth/Security قراردادها.
5. `Application.Behaviors`: validation/logging decorators.

### 4.3 Infrastructure Modules
1. `Infrastructure.Database`: EF, DbContext, migrations.
2. `Infrastructure.Authentication`: JWT, token providers, password hasher.
3. `Infrastructure.Authorization`: policy provider/handler + cache invalidation.
4. `Infrastructure.Logging`: ingestion, retry, integrity.
5. `Infrastructure.Auditing`: checksum chain service.
6. `Infrastructure.Integration`: outbox/inbox persistence + worker.
7. `Infrastructure.Messaging`: RabbitMQ publish/consume.
8. `Infrastructure.Caching`: Redis/memory distributed cache.
9. `Infrastructure.Monitoring`: operational metrics snapshot.
10. `Infrastructure.Security`: structured security event logger.

### 4.4 API Modules
1. `Endpoints.Users` (local auth + OAuth callbacks).
2. `Endpoints.Authorization`.
3. `Endpoints.Todos`.
4. `Endpoints.Logging`.
5. `Endpoints.Audit`.
6. `Endpoints.Observability`.
7. middlewareهای cross-cutting.

### 4.5 Platform Modules
1. `.github/workflows`: CI/CD/Security/Infra/DR/Performance.
2. `infra/terraform`: provisioning محیط‌ها.
3. `deploy/k8s`: blue/green manifests + deploy script.
4. `scripts/dr`: backup/restore/drill.
5. `tests/Performance`: k6 load/soak.

---

## 5) فهرست کامل فیچرهای فعلی (Implemented)
### 5.1 AuthN/AuthZ
1. JWT access token.
2. Refresh token rotation.
3. JWT key rotation (`PreviousSecrets`).
4. Password hashing با Microsoft Identity PasswordHasher.
5. OAuth2 external login (Google/Meta).
6. Lockout پس از خطای لاگین متوالی.
7. RBAC/PBAC.
8. Permission caching با Redis + version invalidation.

### 5.2 Security Hardening
1. Security headers middleware.
2. HSTS در production.
3. HTTPS redirection.
4. Advanced rate limiting (per-user/per-IP).
5. Input sanitization.
6. Security event logs ساخت‌یافته.

### 5.3 Reliability/Data Integrity
1. Outbox/Inbox pattern.
2. RabbitMQ integration.
3. Retry/backoff (Polly).
4. SQL resiliency (`EnableRetryOnFailure`).
5. Log checksum integrity.
6. Audit checksum chain + integrity endpoint.

### 5.4 Observability/Operations
1. Serilog + Seq + ELK.
2. OpenTelemetry tracing/metrics.
3. `/api/v1/observability/metrics`.
4. Operational alert worker (Webhook/PagerDuty-compatible).
5. Health checks (SQL + outbox).

### 5.5 DevOps/Platform
1. CI کامل (restore/build/format/test/publish/scan/image push).
2. Security workflow (CodeQL + dependency + secret scan).
3. CD واقعی (migrate + blue/green + smoke + rollback).
4. Terraform multi-env.
5. DR scripts + scheduled drill workflow.
6. k6 performance workflows.

---

## 6) منطق استفاده از فیچرها (Why)
### 6.1 امنیت
1. OAuth2: کاهش ریسک مدیریت credential داخلی.
2. Lockout + rate limit: کاهش brute-force و credential stuffing.
3. Security headers/HSTS: کاهش حملات لایه وب.
4. Security logs: افزایش detectability و forensic.

### 6.2 پایداری
1. Outbox/Inbox: جلوگیری از loss در failureهای بین DB و broker.
2. Retry/backoff: مقابله با خطاهای transient.
3. Blue/Green: release ایمن با rollback سریع.

### 6.3 انطباق
1. Audit chain integrity: کشف دستکاری و گزارش‌پذیری.
2. Log checksum: اطمینان از صحت شواهد عملیاتی.

### 6.4 کارایی
1. Read context + AsNoTracking: کاهش هزینه queryهای read-heavy.
2. Redis permission cache: کاهش latency در authorization checks.

---

## 7) نگاشت کنترل‌ها به OWASP Top 10
1. `A01 Broken Access Control`: policy-based auth + permission checks.
2. `A02 Cryptographic Failures`: JWT signing + Identity hashing + checksum.
3. `A03 Injection`: sanitization + validation + EF safe querying.
4. `A04 Insecure Design`: layered architecture + bounded modules.
5. `A05 Security Misconfiguration`: secure defaults, HSTS, headers.
6. `A07 Auth Failures`: lockout, refresh rotation, oauth.
7. `A08 Integrity Failures`: outbox/inbox + checksum trails.
8. `A09 Logging/Monitoring Failures`: structured security/operational logs.
9. `A10 SSRF`: outbound governance via managed HttpClient policy.

---

## 8) KPI/SLO عملیاتی
1. Availability: `>= 99.9%`.
2. 5xx rate: `< 1%`.
3. p95 latency (critical endpoints): `< 500ms`.
4. Outbox failed messages: `0` پایدار.
5. Corrupted log rate: `< 0.5%`.
6. MTTR: `< 30min`.
7. حداقل یک DR drill موفق در ماه.

---

## 9) ساختار پروژه پیشنهادی برای رشد آینده
### 9.1 پیشنهاد قطعی
مهاجرت تدریجی به `Vertical Slice + Bounded Module Foldering`:
1. `src/Modules/<Feature>/<Domain|Application|Infrastructure|Api>`
2. DI مستقل در هر ماژول.
3. shared فقط برای abstractionهای واقعاً مشترک.

### 9.2 چرایی
1. کاهش coupling بین تیم‌ها.
2. توسعه سریع‌تر featureهای جدید.
3. تست‌پذیری و ownership واضح‌تر.

---

## 10) تصمیمات بدون ابهام برای توسعه‌دهنده (No-Question Defaults)
1. هر endpoint write باید audit record تولید کند.
2. هر failure امنیتی باید security log ساخت‌یافته داشته باشد.
3. هر query read-heavy باید از `IApplicationReadDbContext` استفاده کند.
4. هر integration بیرونی باید abstraction interface داشته باشد.
5. هر migration باید در CI artifact اسکریپت idempotent تولید کند.
6. هر فیچر جدید باید health/metrics impact داشته باشد.
7. در production فقط secret manager (Key Vault) مجاز است.
8. حد بالای pageSize پیش‌فرض: 200.
9. همه endpointهای حساس باید permission-gated باشند.
10. تغییر schema بدون migration ممنوع.

---

## 11) چک‌لیست پیاده‌سازی فیچر جدید (Developer Playbook)
1. Domain model + rules.
2. Application command/query + validator.
3. Endpoint + mapping.
4. Authorization policy.
5. Audit + security logs.
6. Tests (unit/architecture/integration as needed).
7. Migration (if schema changed).
8. Observability updates (metrics/log tags).
9. Documentation update.
10. CI pass + smoke pass.

---

## 12) راهنمای کامل راه‌اندازی (مرجع یکجا)
### 12.1 پیش‌نیازها
1. .NET 10 SDK
2. Docker + Compose
3. dotnet-ef
4. اختیاری: kubectl, terraform, k6, sqlcmd

### 12.2 Local bootstrap
1. اجرای زیرساخت:
```bash
docker compose up -d sqlserver seq elasticsearch kibana logstash rabbitmq redis
```
2. restore/build:
```bash
dotnet restore CleanArchitecture.slnx
dotnet build CleanArchitecture.slnx
```
3. migration:
```bash
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web.Api/Web.Api.csproj --context Infrastructure.Database.ApplicationDbContext
```
4. run api:
```bash
dotnet run --project src/Web.Api/Web.Api.csproj
```

### 12.3 Local verification
1. `GET /health`
2. `GET /api/v1/observability/metrics`
3. Seq/Kibana/RabbitMQ UI check

### 12.4 OAuth setup
1. ثبت callback: `/api/v1/users/oauth/callback`
2. پر کردن `ExternalOAuth:Google:*` و `ExternalOAuth:Meta:*`
3. تست start endpoint:
1. `/api/v1/users/oauth/google/start`
2. `/api/v1/users/oauth/meta/start`

### 12.5 Production config rules
1. `DatabaseMigrations:ApplyOnStartup=false`
2. `SecretManagement:UseAzureKeyVault=true`
3. `ApiSecurity:UseHsts=true`
4. `RabbitMq:Enabled=true`
5. `RedisCache:Enabled=true`
6. `OperationalAlerting:Enabled=true`

---

## 13) CI/CD Operating Model
### 13.1 CI (`build.yml`)
1. restore/build/format/test
2. publish artifact
3. migration script artifact
4. image scan + push

### 13.2 Security CI (`security.yml`)
1. CodeQL
2. package vulnerability scan
3. secret leak scan

### 13.3 CD (`cd.yml`)
1. set AKS context
2. apply base manifests
3. run DB migration
4. blue/green switch
5. smoke test
6. rollback selector on failure

### 13.4 Infra (`infra.yml`)
1. terraform init/validate/plan

### 13.5 DR (`dr-drill.yml`)
1. scheduled drill
2. backup/restore validation

### 13.6 Performance (`performance.yml`)
1. k6 load/soak execution

---

## 14) DR / Backup / Restore دستورالعمل اجرایی
1. اسکریپت backup: `scripts/dr/backup.ps1`
2. اسکریپت restore: `scripts/dr/restore.ps1`
3. اسکریپت drill: `scripts/dr/drill.ps1`
4. runbook تفصیلی: `docs/operations/backup-restore-runbook.md`

---

## 15) کارهای باقی‌مانده پیشنهادی (اولویت‌دار)
1. MFA برای نقش‌های حساس.
2. Session/device management پیشرفته.
3. SSRF egress allowlist رسمی.
4. Chaos testing pipeline.
5. هزینه‌سنجی زیرساخت (FinOps dashboards).

---

## 16) چک‌لیست Go-Live نهایی
1. همه secrets در Vault تعریف شده‌اند.
2. migrations روی DB مقصد اعمال شده‌اند.
3. OAuth callbacks تایید شده‌اند.
4. health + metrics green هستند.
5. alert delivery تست شده است.
6. DR drill اخیر موفق است.
7. smoke test release پاس است.
8. rollback تمرینی موفق است.

---

## 17) مسئولیت تیمی (RACI ساده)
1. Backend: correctness, modular boundaries, tests.
2. DevOps: pipelines, deploy, infra automation.
3. Security: policy enforcement, scanning, incident controls.
4. SRE/Operations: SLO, alert tuning, capacity, DR readiness.

---

## 18) دستورات پرکاربرد
### Build/Test
```bash
dotnet build CleanArchitecture.slnx
dotnet test CleanArchitecture.slnx --no-build
```

### Migration add
```bash
dotnet ef migrations add <Name> --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web.Api/Web.Api.csproj --context Infrastructure.Database.ApplicationDbContext --output-dir Database/Migrations
```

### Migration update
```bash
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web.Api/Web.Api.csproj --context Infrastructure.Database.ApplicationDbContext
```

### Run API
```bash
dotnet run --project src/Web.Api/Web.Api.csproj
```
