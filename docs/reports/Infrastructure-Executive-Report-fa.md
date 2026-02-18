# گزارش اجرایی فنی زیرساخت (نسخه به‌روز)

**پروژه:** Clean Architecture Base  
**نسخه سند:** 3.1  
**تاریخ:** 2026-02-18  
**وضعیت:** مرجع وضعیت جاری

## 1) خلاصه مدیریتی
پروژه در حال حاضر یک Modular Monolith عملیاتی است با پوشش:
1. AuthN/AuthZ پیشرفته
2. Files (MinIO)
3. Notifications چندکاناله
4. Logging/Auditing/Observability
5. CI/CD + DR + Performance assets

## 2) وضعیت معماری

### لایه‌ها
- Domain: `src/Domain/Modules/*`
- Application: `src/Application/Modules/*`
- Infrastructure: `src/Infrastructure/Modules/*` + cross-cutting roots
- API: `src/Web.Api/Endpoints/Modules/*`

### اصل معماری
1. ماژول‌محوری در ساختار پوشه‌ای اعمال شده است.
2. cross-cuttingها در ریشه‌های مشترک باقی مانده‌اند.
3. registration ماژول‌ها در `Infrastructure/DependencyInjection.cs` orchestration می‌شود.

## 3) ماژول‌های عملیاتی فعلی
1. Users
2. Authorization/Auth
3. Todos
4. Files
5. Notifications
6. Logging
7. Audit
8. Observability

## 4) قابلیت‌های کلیدی فعال

### امنیت
- JWT + Refresh Token rotation
- Lockout policy
- OAuth external login
- Permission-based authorization
- Security headers + rate limiting

### قابلیت اطمینان
- Outbox/Inbox
- RabbitMQ integration
- SQL resiliency
- Background workers

### عملیات
- Health checks
- Operational metrics endpoint
- Seq + ELK
- DR scripts + runbook

### فایل
- MinIO object storage
- Validation + malware scanner abstraction
- ACL + access audit + temporary links

### اعلان
- Notification dispatcher + worker
- Retry/backoff
- Template/schedule/report/ACL APIs
- Channel senders: Email/SMS/Slack/Teams/Push/InApp

## 5) وضعیت کیفیت
- `dotnet build`: پاس
- `dotnet test --no-build`: پاس
- ساختار ماژولی جدید بدون break در API اصلی اعمال شده است.

## 6) شکاف‌های اصلی
1. module boundary tests برای enforce دقیق coupling.
2. Scheduler module مستقل.
3. RuleEngine module برای triggerهای business-driven.
4. security hardening تکمیلی (MFA, session mgmt, SSRF allowlist).

## 7) تصمیمات اجرایی پیشنهادی
1. اولویت 1: معماری ماژولی را با test enforce کنید.
2. اولویت 2: scheduler/rule-engine را به صورت ماژول مستقل اضافه کنید.
3. اولویت 3: Notification provider hardening و monitoring per-channel تکمیل شود.

## 8) مسیرهای مرجع
- معماری ماژولی: `docs/reports/Modular-Monolith-Structure-fa.md`
- راه‌اندازی سریع: `docs/operations/Quick-Run-Guide-fa.md`
- کانفیگ‌های لازم: `docs/operations/Run-Required-Config-fa.md`
- فایل‌ها: `docs/reports/Notification-FSD-v2-fa.md`
