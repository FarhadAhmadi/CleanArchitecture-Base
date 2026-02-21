# ماژول Notifications

تاریخ به‌روزرسانی: 2026-02-21

## هدف
ارسال اعلان چندکاناله با template، schedule، ACL و گزارش‌گیری.

## مسئولیت‌های اصلی
- ایجاد/خواندن/آرشیو اعلان
- مدیریت template و revision
- زمان‌بندی و لغو ارسال
- مدیریت permission در سطح اعلان
- تولید گزارش summary/details
- اجرای dispatch با retry/backoff

## مدل دامنه
- `NotificationMessage`
- `NotificationTemplate`
- `NotificationTemplateRevision`
- `NotificationSchedule`
- `NotificationPermissionEntry`
- `NotificationDeliveryAttempt`

## Use caseهای کلیدی
- `CreateNotificationCommand`
- `ScheduleNotificationCommand`
- `CreateNotificationTemplateCommand`
- `UpdateNotificationTemplateCommand`
- `DeleteNotificationTemplateCommand`
- `ListNotificationsQuery`, `GetNotificationQuery`
- `GetNotificationSummaryReportQuery`, `GetNotificationDetailsReportQuery`

## API و سطح دسترسی
- مسیرها: `/api/v1/notifications/*` و `/api/v1/notification-templates/*`
- `notifications.read`, `notifications.write`
- `notifications.templates.manage`
- `notifications.schedules.manage`
- `notifications.permissions.manage`
- `notifications.reports.read`

## وابستگی‌ها
- providerهای کانال (Email/SMS/Slack/Teams/Push/InApp)
- مکانیزم رمزنگاری/هش recipient

## داده و نگهداشت
- اسکیما: `notifications`
- تلاش‌های ارسال (attempts) برای troubleshooting و SLA tracking ذخیره می‌شود.

## نکات عملیاتی
- credential providerها باید دوره‌ای rotate شوند.
- مانیتور نرخ خطای delivery برای هر کانال ضروری است.

## ریسک‌ها
- اختلال provider بیرونی مستقیما روی delivery impact می‌گذارد.
