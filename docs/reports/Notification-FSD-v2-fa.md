# Notification FSD v2.1 (Implemented)

**Feature:** NotificationComponent  
**Version:** 2.1  
**Updated:** 2026-02-18

## 1) Scope
ماژول اعلان برای ارسال چندکاناله، مدیریت قالب، زمان‌بندی، ACL، گزارش و آرشیو پیاده‌سازی شده است.

## 2) موقعیت ماژول در ساختار جدید
- Domain: `src/Domain/Modules/Notifications`
- Infrastructure: `src/Infrastructure/Modules/Notifications/Infrastructure/Notifications`
- EF Configs: `src/Infrastructure/Database/Configurations/Notifications`
- API: `src/Web.Api/Endpoints/Modules/Notifications`

## 3) API های پیاده‌سازی‌شده
- `POST /api/v1/notifications`
- `GET /api/v1/notifications/{notificationId}`
- `GET /api/v1/notifications`
- `POST /api/v1/notifications/{notificationId}/schedule`
- `GET /api/v1/notifications/schedules`
- `DELETE /api/v1/notifications/schedules/{scheduleId}`
- `POST /api/v1/notifications/{notificationId}/permissions`
- `GET /api/v1/notifications/{notificationId}/permissions`
- `DELETE /api/v1/notifications/archive/{id}`
- `GET /api/v1/notifications/reports/summary`
- `GET /api/v1/notifications/reports/details`
- `POST /api/v1/notification-templates`
- `GET /api/v1/notification-templates/{templateId}`
- `PUT /api/v1/notification-templates/{templateId}`
- `DELETE /api/v1/notification-templates/{templateId}`
- `GET /api/v1/notification-templates`

## 4) Data Model
- `NotificationMessage`
- `NotificationTemplate`
- `NotificationTemplateRevision`
- `NotificationSchedule`
- `NotificationPermissionEntry`
- `NotificationDeliveryAttempt`

## 5) امنیت
1. رمزنگاری recipient با `NotificationSensitiveDataProtector`.
2. ذخیره hash پایدار recipient برای report/filter بدون افشای داده.
3. ACL در سطح اعلان (User/Role).
4. مجوزهای دسترسی اختصاصی Notifications در `PermissionCodes`.

## 6) Dispatch و Reliability
1. Worker پس‌زمینه: `NotificationDispatchWorker`.
2. Dispatcher مرکزی: `NotificationDispatcher`.
3. retry/backoff بر اساس `MaxRetries` و `BaseRetryDelaySeconds`.
4. ثبت attemptها در `NotificationDeliveryAttempt`.

## 7) Channel Senders
Senderهای واقعی برای کانال‌های زیر پیاده شده‌اند:
- Email (SMTP)
- SMS (HTTP provider)
- Slack (Webhook/API)
- Teams (Webhook)
- Push (HTTP provider)
- InApp

## 8) کانفیگ
تنظیمات در `Notifications` داخل `appsettings*.json`:
- هسته: `Enabled`, `MaxRetries`, `DispatchBatchSize`, `DispatchPollingSeconds`, `SensitiveDataEncryptionKey`
- کانال‌ها: `Email`, `Sms`, `Slack`, `Teams`, `Push`, `InApp`

## 9) نکته اجرایی
برای ارسال واقعی، باید provider credentials هر کانال مقداردهی شود. در غیر این صورت dispatch به صورت controlled fail ثبت می‌شود.

## 10) DoD فعلی
- Build: پاس
- Tests: پاس
- Endpointها: permission-gated
- Migration: اعمال‌شده در پروژه
- ساختار ماژولی: اعمال‌شده
