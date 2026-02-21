# پلتفرم مشترک (Cross-cutting)

تاریخ به‌روزرسانی: 2026-02-21

## هدف
ارائه زیرساخت فنی مشترک برای همه ماژول‌ها با حداقل تکرار و حداکثر پایداری.

## اجزای اصلی
- `Core`: time provider، domain event dispatcher
- `DataAccess`: DbContext، migration، SQL resilience
- `Caching`: Redis/Memory و versioning cache
- `Integration`: outbox/inbox، workerهای پیام
- `HealthChecks`: health probes و readiness
- `Shared`: primitiveها و قراردادهای مشترک

## نقش معماری
- یکسان‌سازی الگوهای فنی بین ماژول‌ها
- کاهش coupling مستقیم ماژول‌ها
- افزایش قابلیت عملیات و عیب‌یابی

## وابستگی‌های عملیاتی
- SQL Server
- RabbitMQ
- Redis
- ابزارهای observability (مانند Seq/Elastic/OpenTelemetry)

## کنترل کیفیت
- لایه‌بندی با تست معماری enforce می‌شود:
  - Domain مستقل از Application/Infrastructure/Web
  - Application مستقل از Infrastructure/Web
  - Infrastructure مستقل از Web

## ریسک‌ها
- بزرگ‌شدن بیش از حد اجزای مشترک می‌تواند به God-module منجر شود.
- ناهماهنگی نسخه dependencyهای زیرساختی روی همه ماژول‌ها اثر مستقیم دارد.
