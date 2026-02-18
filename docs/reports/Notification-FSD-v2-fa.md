# Notification FSD v2.0

## 1) هدف و دامنه
- ماژول `Notifications` برای ارسال اعلان چندکاناله (Email/SMS/Push/In-App/Slack/Teams)
- پشتیبانی از:
`Template`, `Schedule`, `ACL`, `Retry/Backoff`, `Audit/Report`, `Archive`

## 2) API نهایی
- `POST /api/v1/notifications`
- `GET /api/v1/notifications/{notificationId}`
- `GET /api/v1/notifications`
- `POST /api/v1/notification-templates`
- `GET /api/v1/notification-templates/{templateId}`
- `PUT /api/v1/notification-templates/{templateId}`
- `DELETE /api/v1/notification-templates/{templateId}`
- `GET /api/v1/notification-templates`
- `POST /api/v1/notifications/{notificationId}/schedule`
- `GET /api/v1/notifications/schedules`
- `DELETE /api/v1/notifications/schedules/{scheduleId}`
- `POST /api/v1/notifications/{notificationId}/permissions`
- `GET /api/v1/notifications/{notificationId}/permissions`
- `DELETE /api/v1/notifications/archive/{id}`
- `GET /api/v1/notifications/reports/summary`
- `GET /api/v1/notifications/reports/details`

## 3) مدل داده
- `NotificationMessages`
- `NotificationTemplates`
- `NotificationTemplateRevisions`
- `NotificationSchedules`
- `NotificationPermissionEntries`
- `NotificationDeliveryAttempts`

## 4) امنیت
- رمزگذاری Recipient با `AES` در `NotificationSensitiveDataProtector`
- Hash پایدار Recipient برای جستجو/گزارش بدون افشای داده
- ACL در سطح اعلان (`User/Role`)
- Rate-limit داخلی ایجاد اعلان به ازای کاربر

## 5) قابلیت اطمینان
- Worker پس‌زمینه: `NotificationDispatchWorker`
- Dispatch strategy بر اساس کانال
- Retry با Exponential Backoff تا `MaxRetries`

## 6) کانفیگ
- بخش `Notifications` در `appsettings*.json`
- فیلدهای کلیدی: `MaxRetries`, `DispatchBatchSize`, `DispatchPollingSeconds`, `PerUserPerMinuteLimit`, `SensitiveDataEncryptionKey`

## 7) DoD
- Build/Test/Format پاس
- Migration دیتابیس تولید شده
- Contract snapshot به‌روزرسانی شده
- Endpointها تحت permission policy قرار گرفته‌اند
