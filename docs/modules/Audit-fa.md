# ماژول Audit

تاریخ به‌روزرسانی: 2026-02-21

## هدف
ایجاد trail تغییرات حساس به‌صورت tamper-evident برای نیازهای امنیتی و انطباق.

## مسئولیت‌های اصلی
- ثبت رکورد audit با hash payload
- اتصال رکوردها با checksum chain
- گزارش یکپارچگی (integrity report)
- جستجو/صفحه‌بندی trail

## مدل دامنه
- `AuditEntry`

## Use caseهای کلیدی
- `GetAuditEntriesQuery`
- `GetAuditIntegrityReportQuery`

## API و سطح دسترسی
- مسیرها: `/api/v1/audit/*`
- `audit.read`

## وابستگی‌ها
- مصرف توسط سایر ماژول‌ها هنگام رخدادهای حساس

## داده و نگهداشت
- اسکیما: `audit`
- برای حجم بلندمدت باید policy آرشیو تعریف شود.

## نکات عملیاتی
- integrity check دوره‌ای باید در runbook عملیات ثبت شود.

## ریسک‌ها
- افزایش حجم داده و هزینه نگهداشت در دوره طولانی.
