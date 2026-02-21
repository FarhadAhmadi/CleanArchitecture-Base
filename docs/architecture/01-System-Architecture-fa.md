# سند معماری سیستم (نسخه اجرایی)

تاریخ: 2026-02-21  
مبنای تحلیل: commit `6db4c27`

## 1) خلاصه مدیریتی
این محصول یک **Modular Monolith** مبتنی بر **Clean Architecture** است که در یک سرویس اصلی (`Web.Api`) اجرا می شود و ماژول های کسب وکاری را با مرزهای مشخص در لایه های Domain/Application/Infrastructure نگه می دارد.

سیستم برای مرحله رشد محصول طراحی شده است:
- سرعت توسعه بالا (یک deployable)
- کنترل امنیت و مجوزدهی دقیق (Role + Permission)
- قابلیت اطمینان عملیاتی (Outbox/Inbox + retry + health)
- مشاهده پذیری عملیاتی (metrics + logging + alerting)

## 2) سبک معماری و دلیل انتخاب

### سبک اصلی
- Modular Monolith
- Clean Architecture
- CQRS سبک (Command/Query handlers)
- Domain Events + Outbox/Inbox

### چرا این انتخاب منطقی است
- نسبت به Microservice، هزینه عملیاتی پایین تر و تحویل سریع تر دارد.
- نسبت به Monolith سنتی، مرزبندی ماژول ها و امنیت/پایداری بسیار بهتر است.
- برای تبدیل تدریجی به سرویس های مستقل در آینده، مسیر مهاجرت مشخص نگه داشته شده است.

## 3) ساختار لایه ای اجرا شده در کد
- Domain: `src/Domain/Modules/*`
- Application: `src/Application/Modules/*` و `src/Application/Abstractions/*`
- Infrastructure: `src/Infrastructure/*` و `src/Infrastructure/Modules/*`
- API/Presentation: `src/Web.Api/*`
- SharedKernel: `src/SharedKernel/*`

قواعد معماری با تست enforce شده اند:
- Domain به Application/Infrastructure/Web وابسته نیست.
- Application به Infrastructure/Web وابسته نیست.
- Infrastructure به Web وابسته نیست.

مرجع: `tests/ArchitectureTests/Layers/LayerTests.cs`

## 4) Composition Root و ماژول بندی
Composition Root از `Program.cs` و `Infrastructure.DependencyInjection` انجام می شود.

زنجیره ثبت سرویس:
- `AddApplication()`
- `AddPresentation(configuration)`
- `AddInfrastructure(configuration)`

ماژول های Infrastructure ثبت شده:
- Core
- DataAccess
- Caching
- Integration
- HealthChecks
- Logging
- Users
- Auth
- Authorization
- Audit
- Monitoring
- Files
- Notifications
- Scheduler

مرجع: `src/Web.Api/Program.cs`, `src/Infrastructure/DependencyInjection.cs`

## 5) ماژول های کسب وکاری فعال
- Users/Auth
- Authorization
- Todos
- Profiles
- Files
- Notifications
- Logging
- Audit
- Observability
- Scheduler (job orchestration)

## 5.1) وضعیت فعلی Scheduler
- endpointهای scheduler در `src/Web.Api/Endpoints/Modules/Scheduler/*` فعال هستند.
- worker زمان‌بند در `src/Infrastructure/Modules/Scheduler/Infrastructure/Scheduling/SchedulerWorker.cs` فعال است.
- قابلیت‌های runtime شامل retry, misfire policy, replay, quarantine و dependency check است.

جزئیات هر ماژول در `docs/architecture/03-Module-Catalog-fa.md` آمده است.

## 6) معماری داده و مرزهای Schema
سیستم یک دیتابیس اصلی SQL Server دارد اما با schema-based modularization.

| Schema | هدف |
|---|---|
| `users` | داده های کاربر و external login |
| `auth` | RBAC/Permissions/RefreshTokens/Identity tables |
| `todos` | Todo domain |
| `profiles` | Profile domain |
| `files` | فایل ها، ACL فایل، audit فایل |
| `notifications` | اعلان، template، schedule، attempts |
| `logging` | رخدادهای لاگ، rule، incident |
| `audit` | audit trail tamper-evident |
| `integration` | outbox/inbox |
| `dbo` | پیش فرض EF و metadata |

مرجع: `src/Infrastructure/Database/Schemas.cs` و `src/Infrastructure/Database/Configurations/*`

## 7) امنیت و کنترل دسترسی

### Authentication
- JWT Bearer
- Refresh Token rotation + revoke + reuse detection
- Lockout policy
- OAuth (Google/Meta)

مرجع: `src/Infrastructure/Modules/Auth/AuthModule.cs`, `src/Application/Modules/Users/*`

### Authorization
- Dynamic policy-based permission (`RequireAuthorization(permissionCode)`)
- Role + Permission model
- Permission cache versioning روی Redis/Memory

مرجع: `src/Infrastructure/Modules/Authorization/Infrastructure/Authorization/*`

### Hardening
- Security headers (CSP, X-Frame-Options, ...)
- حذف `Server` header
- Request hardening (header count/size/content-type)
- Rate limiting سراسری + rate limit ویژه لینک عمومی فایل
- Global exception handling بدون افشای اطلاعات حساس

مرجع: `src/Web.Api/Middleware/*`, `src/Web.Api/DependencyInjection.cs`, `src/Web.Api/Infrastructure/GlobalExceptionHandler.cs`

### Audit
- Audit trail chain (checksum + previous checksum)
- Integrity report endpoint

مرجع: `src/Infrastructure/Modules/Audit/Infrastructure/Auditing/AuditTrailService.cs`, `src/Application/Modules/Audit/*`

## 8) قابلیت اطمینان و پایداری

### Reliable messaging
- Outbox برای انتشار eventهای دامنه
- Inbox برای مصرف idempotent پیام ها
- RabbitMQ publisher/consumer workers

مرجع: `src/Infrastructure/Integration/*`, `src/Infrastructure/Messaging/*`

### Retry/Resilience
- SQL retry (`EnableRetryOnFailure`)
- Polly pipelines در background workers
- HTTP resilience handler

مرجع: `src/Infrastructure/Modules/DataAccessModule.cs`, `src/Infrastructure/*Worker.cs`, `src/Web.Api/DependencyInjection.cs`

### Health & Ops
- `/health`
- Outbox health check
- Operational metrics + orchestration health + replay endpoints
- Operational alert worker (Webhook/PagerDuty)

مرجع: `src/Infrastructure/Modules/HealthChecksModule.cs`, `src/Application/Modules/Observability/*`, `src/Web.Api/Infrastructure/OperationalAlertWorker.cs`

## 9) Observability
- Structured logging با Serilog
- Sink به Seq یا Elasticsearch
- OpenTelemetry tracing + metrics (OTLP optional)
- Logging platform داخلی (ingest, schema, validation, alerts)
- Logging Decorator در Application برای ثبت شروع/پایان/خطا و slow operation فعال است.
- Domain event dispatch logging برای شمارش handlerها و رویدادهای بدون handler فعال است.

مرجع: `src/Web.Api/Program.cs`, `src/Web.Api/DependencyInjection.cs`, `src/Infrastructure/Modules/Logging/*`

## 10) استقرار و عملیات

### Local/dev
- Docker Compose stack: SQL Server, RabbitMQ, Redis, MinIO, Seq, Elasticsearch, Kibana, Logstash, WebApi
- اسکریپت یکپارچه اجرا: `scripts/dev/dev-stack.ps1`

### CI/CD
- CI: restore/build/format/test/publish + migration artifact + image scan/push
- CD: AKS deploy + blue/green + smoke test + rollback on failure
- Security: CodeQL + NuGet vuln scan + Gitleaks
- DR drill: زمان بندی هفتگی
- Infra: Terraform plan/validate

مرجع: `.github/workflows/*.yml`, `deploy/k8s/*`, `infra/terraform/*`

## 11) کنترل کیفیت و شواهد فنی
- Architecture tests
- Security tests
- Module integration tests (Users, Profiles, Notifications, Audit)
- OpenAPI snapshot contract tests
- k6 load/soak assets
- تست رفتار permission برای endpointهای scheduler

مرجع: `tests/ArchitectureTests/*`, `tests/ContractTests/*`, `tests/Performance/*`

## 12) مزایا، هزینه ها و trade-off
| محور | مزیت | هزینه/ریسک | کنترل فعلی |
|---|---|---|---|
| Modular Monolith | سرعت تحویل بالا + سادگی عملیات | احتمال coupling تدریجی | لایه بندی + تست معماری |
| RBAC+Permission | کنترل دسترسی دقیق | پیچیدگی مدیریت نقش/مجوز | seeding + endpoint permission gating |
| Outbox/Inbox | کاهش خطا در تعاملات async | نگهداری worker و queue | health + replay + retry |
| Logging Platform داخلی | قابلیت تحلیل/alert عمیق | هزینه نگهداری نسبت به SaaS | schema + ACL + queue health |
| File/Public Link | تجربه اشتراک سریع | ریسک misuse لینک | signed token + rate limit + audit log |
| Notification چندکاناله | انعطاف کانال ارتباطی | نیاز به credential و hardening provider | options validation + retry + attempts tracking |

## 13) ریسک های مهم برای سهام داران

### ریسک 1: Bootstrap admin ثابت در seeder
در `AuthorizationSeeder` ایمیل/پسورد bootstrap به صورت hard-coded وجود دارد.
- اثر: ریسک امنیتی بحرانی در صورت استفاده ناخواسته در محیط واقعی
- اقدام فوری: حذف credential ثابت، انتقال به secret manager و provision امن

### ریسک 2: کلیدهای پیش فرض در config نمونه
کلیدهایی مانند `Notifications:SensitiveDataEncryptionKey` در نمونه config نیاز به override قطعی در production دارند.
- اثر: ریسک افشای داده/ضعف رمزنگاری
- اقدام: enforce policy در startup و CI

### ریسک 3: رشد دامنه در یک deployable واحد
با افزایش تیم/دامنه، build/deploy time و coupling ممکن است رشد کند.
- اقدام: ادامه module boundary tests و تعریف migration plan برای سرویس سازی انتخابی

## 14) جمع بندی اجرایی
معماری فعلی برای فاز «رشد کنترل شده با تمرکز بر امنیت و قابلیت اطمینان» مناسب و قابل دفاع است.  
برای ورود به مقیاس بزرگتر، سه اقدام اولویت بالا:
1. سخت گیری امنیتی در secrets/bootstrap
2. تقویت governance مرز ماژول ها
3. تعریف roadmap سرویس سازی انتخابی بر اساس داده های واقعی بار و تیم
