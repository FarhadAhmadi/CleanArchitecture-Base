# نقشه راه فنی (به‌روز بر اساس وضعیت فعلی)

**پروژه:** Clean Architecture Base  
**نسخه:** 1.1  
**تاریخ:** 2026-02-18

## 1) وضعیت فعلی
موارد زیر انجام شده‌اند:
1. ساختار پروژه به الگوی ماژولی منتقل شده است.
2. DI به صورت ماژولار در Infrastructure تفکیک شده است.
3. ماژول Files و Notifications اضافه شده‌اند.
4. Entity Configurations در `src/Infrastructure/Database/Configurations` متمرکز شده‌اند.
5. اسکریپت scaffolding فیچر با ساختار جدید هماهنگ شده است.
6. راه‌اندازی سریع پروژه با `scripts/dev/dev-stack.ps1` اضافه شده است.

## 2) اولویت‌های باقی‌مانده (P0/P1)

### P0-01 Module Boundary Tests
**اقدام:** افزودن تست معماری برای جلوگیری از dependency غیرمجاز بین ماژول‌ها.  
**DoD:** هر نقض boundary در CI fail شود.

### P0-02 Notification Provider Hardening
**اقدام:** health-check per provider + retry policy per channel + dead-letter strategy.  
**DoD:** failureهای provider قابل رصد و قابل بازیابی باشند.

### P0-03 Scheduler Module
**اقدام:** پیاده‌سازی ماژول زمان‌بندی مستقل برای jobهای delayed/interval/cron-based.  
**Scope MVP:**
1. Job CRUD + فعال/غیرفعال
2. Schedule CRUD (Cron/Interval/One-time + Start/End)
3. Run/Pause/Resume
4. Execution logs + status model
5. Permission/ACL + Audit trail
6. Integration با Notification برای failure/delay alert
**API Target (v1):**
- `/api/v1/scheduler/jobs`
- `/api/v1/scheduler/jobs/{jobId}/schedule`
- `/api/v1/scheduler/jobs/{jobId}/run|pause|resume`
- `/api/v1/scheduler/jobs/{jobId}/logs`
**DoD:**
1. اعلان‌های زمان‌بندی‌شده وابسته به endpoint دستی نباشند.
2. حداقل 1000 job فعال بدون افت عملکرد بحرانی مدیریت شود.
3. لاگ/متریک/آلارم اجرایی برای Jobها در observability قابل مشاهده باشد.
4. سناریوی failover پایه در محیط multi-node تست شود.

### P1-01 RuleEngine Module
**اقدام:** موتور Rule برای trigger اعلان و workflow داخلی.  
**DoD:** rule-driven notification در سطح module قابل مدیریت باشد.

### P1-02 Contract Governance
**اقدام:** version compatibility policy و contract tests دقیق‌تر برای API v1.  
**DoD:** breaking change بدون تایید، pipeline را fail کند.

### P1-03 Security Hardening Backlog
**اقدام:** MFA برای نقش‌های حساس + Session Management + SSRF Allowlist.  
**DoD:** کنترل‌های امنیتی یادشده فعال و تست‌پذیر شوند.

## 3) DX Roadmap

### DX-01 Modular DI Refactor
**وضعیت:** Done  
**نتیجه:** registration سرویس‌ها در moduleهای مستقل انجام می‌شود.

### DX-02 Feature Scaffolding CLI
**وضعیت:** Done (v1)  
**مسیر:** `scripts/feature/new-feature.ps1`  
**نیاز بعدی:** افزودن template برای testهای module boundary.

### DX-03 Pre-commit Quality Gate
**وضعیت:** In progress  
**اقدام:** hook شامل `dotnet format --verify-no-changes`, `dotnet build`, `dotnet test --no-build`.

### DX-04 Contract Tests for API Compatibility
**وضعیت:** In progress  
**اقدام:** enforce سخت‌گیرانه snapshot update policy در CI.

## 4) ترتیب اجرای پیشنهادی (30/60/90)

### 30 روز
1. Module Boundary Tests
2. Pre-commit quality gate
3. Notification provider hardening

### 60 روز
1. Scheduler Module
2. RuleEngine Module (MVP)
3. Contract governance تکمیلی

### 90 روز
1. MFA + Session management
2. SSRF allowlist
3. Chaos/performance scenarios for modular runtime

## 5) معیار موفقیت
1. Lead time توسعه فیچر جدید کمتر از 2 روز کاری.
2. Failure ناشی از coupling بین ماژول‌ها نزدیک به صفر.
3. API compatibility incidents صفر در releaseهای minor.
4. p95 مسیرهای critical کمتر از 500ms در بار هدف.
