# فناوری ها و اثرات اجرایی

تاریخ: 2026-02-21  
مبنای تحلیل: commit `6db4c27`

## 1) پلتفرم اصلی
- Target Framework: `net10.0`
- Solution: `Application`, `Domain`, `Infrastructure`, `SharedKernel`, `Web.Api`

مرجع: `Directory.Build.props`, `CleanArchitecture.slnx`

## 2) ماتریس فناوری، اثر، ریسک

| فناوری | نسخه/وضعیت | نقش در سیستم | اثر مثبت | هزینه/ریسک | کنترل فعلی |
|---|---|---|---|---|---|
| .NET / ASP.NET Core | 10.x / net10.0 | هسته اجرای API | کارایی بالا، پشتیبانی کامل ابزار | وابستگی به نسخه جدید | CI و Docker version pinning |
| Minimal APIs | فعال | endpoint-driven API | سادگی routing و توسعه سریع | پراکندگی logic در endpointها در صورت عدم انضباط | جداسازی handler/service و mapping tests |
| EF Core + SQL Server | EF 10 + SQL 2022 | persistence | مدل داده قوی و migration کنترل شده | lock/contention و هزینه scale عمودی | retry strategy + migration artifact |
| Identity Core + JWT | فعال | AuthN | استاندارد صنعتی، token-based auth | مدیریت کلید و lifecycle پیچیده | option validation + key rotation support |
| Role/Permission Authorization | فعال | AuthZ | کنترل دسترسی granular | پیچیدگی governance مجوزها | seeder + dynamic policy provider |
| FluentValidation | 12.x | validation | کاهش خطاهای ورودی | افزایش کد validator | decorators + matrix tests |
| Scrutor | 7.x | DI scanning | کاهش wiring دستی | احتمال registration ناخواسته | assembly-scoped scanning |
| Serilog | 4.x | structured logging | تحلیل عملیاتی بهتر | حجم لاگ و هزینه نگهداری | level configs + sink strategy |
| Seq | docker service | log search | setup سریع dev/ops | single-node dependency | قابل جایگزینی با ELK |
| Elasticsearch/Kibana/Logstash | 8.17.x | observability stack | تحلیل پیشرفته لاگ | RAM/ops cost بالا | optional/segmented deployment |
| OpenTelemetry (OTLP) | 1.11.x | tracing/metrics | استاندارد vendor-neutral | overhead telemetry | OTLP endpoint optional |
| Polly + Http Resilience | 8.x | resilience | تحمل خطا بهتر | retry storm در config غلط | bounded retries + timeouts |
| RabbitMQ | 3.13 | integration events | async decoupling | complexity + operational runbook | Outbox/Inbox + health/replay |
| Redis | 7.4 | distributed cache | performance بهتر در authz | consistency/version invalidation | cache version service |
| MinIO | latest | object storage | S3-compatible local/prod-like | credential و lifecycle object | options validation + audit |
| ClamAV scanner | optional | malware scan | کاهش ریسک فایل آلوده | latency و infra cost | toggle by config + no-op fallback |
| Notification channel senders | چندگانه | outbound comms | انعطاف ارتباطی | dependency به providerهای بیرونی | retry/backoff + attempts tracking |
| Swagger/OpenAPI | Swashbuckle 10.x | API contract | self-documenting API | drift بین code و contract | snapshot contract tests |
| xUnit + NetArchTest + Shouldly | فعال | quality gates | کنترل regression/architecture | زمان اجرای تست | CI parallel jobs |
| k6 | active scripts | performance validation | تست load/soak قابل تکرار | نیاز به محیط هدف پایدار | workflow دستی/کنترلی |
| Docker Compose | فعال | local stack | onboarding سریع | مصرف منابع local بالا | dev-stack script + port checks |
| Kubernetes Blue/Green | فعال | deployment strategy | کاهش downtime و rollback سریع | پیچیدگی release process | smoke test + automatic rollback |
| Terraform (Azure) | فعال | IaC | repeatable infra | drift و secret handling | validate/plan workflows |
| CodeQL/Trivy/Gitleaks | فعال | security pipeline | کشف آسیب پذیری و secrets | false positives | policy review در CI |

## 3) فناوری های اختیاری/مشروط
- Azure Key Vault (`SecretManagement:UseAzureKeyVault`)
- PagerDuty/Webhooks (`OperationalAlerting`)
- External OAuth providers (Google/Meta)
- ClamAV

این موارد برای deployment profileهای متفاوت (dev/staging/prod) با config فعال/غیرفعال می شوند.

## 4) اثرات کسب وکاری کلان
- **Time-to-market:** بالا (به دلیل monolith ماژولار)
- **Operational Safety:** بالا (به دلیل health/retry/outbox/inbox)
- **Security Posture:** خوب، با نیاز به سخت گیری بیشتر روی secrets/bootstrap
- **Scalability Path:** مناسب برای scale-up و مهاجرت انتخابی به service decomposition

## 5) تصمیمات پیشنهادی سطح هیئت مدیره
1. سرمایه گذاری روی secret governance (اجباری در production).
2. ادامه توسعه روی همین معماری تا رسیدن به شاخص های بار/تیم مشخص.
3. تعریف milestone رسمی برای service extraction فقط برای ماژول های پرمصرف.
