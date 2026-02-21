# ماژول Logging

تاریخ به‌روزرسانی: 2026-02-21

## هدف
فراهم کردن پلتفرم لاگ داخلی با اعتبارسنجی schema، هشداردهی و کنترل دسترسی.

## مسئولیت‌های اصلی
- ingest رخداد (single/bulk)
- validate/transform ورودی
- بازیابی رخدادها و corrupted events
- soft delete رخداد
- مدیریت alert rule و incident
- کنترل دسترسی اختصاصی بخش لاگ

## مدل دامنه
- `LogEvent`
- `AlertRule`
- `AlertIncident`
- `AlertIncidentCreatedDomainEvent`

## Use caseهای کلیدی
- `IngestSingleLogCommand`, `IngestBulkLogsCommand`
- `GetLogEventsQuery`, `GetCorruptedLogEventsQuery`
- `CreateAlertRuleCommand`, `UpdateAlertRuleCommand`, `DeleteAlertRuleCommand`
- `GetAlertIncidentsQuery`
- `AssignLoggingAccessCommand`

## API و سطح دسترسی
- مسیرها: `/api/v1/logging/*`
- `logging.events.write`, `logging.events.read`, `logging.events.delete`
- `logging.alerts.manage`
- `logging.access.manage`
- `logging.export.read`

## وابستگی‌ها
- ماژول Notifications برای dispatch برخی alertها
- صف‌ها و workerهای داخلی برای retry

## داده و نگهداشت
- اسکیما: `logging`

## نکات عملیاتی
- health endpointهای logging باید در مانیتورینگ مرکزی پایش شوند.
- retention policy لاگ باید با الزامات امنیتی/حقوقی هم‌راستا باشد.

## ریسک‌ها
- اشباع صف ingest در بار بالا و افزایش تاخیر پردازش.
