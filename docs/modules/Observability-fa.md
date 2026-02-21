# ماژول Observability

تاریخ به‌روزرسانی: 2026-02-21

## هدف
ارائه نمای عملیاتی سیستم شامل KPI، سلامت orchestration و ابزار replay شکست‌ها.

## مسئولیت‌های اصلی
- ارائه snapshot متریک‌های عملیاتی
- ارائه سلامت orchestration
- ارائه catalog قراردادهای رویداد
- replay پیام‌های شکست‌خورده inbox/outbox

## Use caseهای کلیدی
- `GetOperationalMetricsQuery`
- `GetOrchestrationHealthQuery`
- `GetEventCatalogQuery`
- `ReplayFailedOutboxCommand`
- `ReplayFailedInboxCommand`

## API و سطح دسترسی
- مسیرها: `/api/v1/observability/*` و `/api/v1/dashboard/*`
- `observability.read`
- `observability.manage`

## وابستگی‌ها
- Integration (outbox/inbox)
- Logging و Notifications برای شاخص‌های سلامت

## داده و نگهداشت
- تمرکز ماژول روی خواندن state عملیاتی و اجرای actionهای replay است.

## نکات عملیاتی
- replay باید با SOP مشخص و ثبت audit انجام شود.
- شاخص SLO باید روی dashboard عملیاتی تعریف و پیگیری شود.

## ریسک‌ها
- replay اشتباه می‌تواند باعث تکرار اثرات جانبی در سیستم شود.
