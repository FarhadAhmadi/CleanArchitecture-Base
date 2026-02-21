# ماژول Scheduler

تاریخ به‌روزرسانی: 2026-02-21

## هدف
زمان‌بندی و اجرای قابل اتکای Jobها با پشتیبانی از `cron`، `interval` و `one-time` همراه با کنترل عملیاتی.

## وضعیت پیاده‌سازی
این ماژول در کد فعال است و شامل endpoint، domain model، worker، distributed lock، retry و quarantine است.

## مسئولیت‌های اصلی
- CRUD Job و فعال/غیرفعال‌سازی
- تعریف Schedule با policy کنترل misfire
- اجرای دستی، pause/resume، replay dead-letter
- قرنطینه Jobهای پرخطا
- مدیریت وابستگی بین Jobها
- مدیریت permission سطح Job
- ثبت لاگ اجرا و گزارش خروجی (CSV/PDF)

## مدل دامنه
- `ScheduledJob`
- `JobSchedule`
- `JobDependency`
- `JobExecution`
- `JobPermissionEntry`
- `SchedulerLockLease`

## Use caseهای کلیدی
- `CreateJobCommand`, `UpdateJobCommand`, `DisableJobCommand`
- `UpsertJobScheduleCommand`, `DeleteJobScheduleCommand`, `GetJobScheduleQuery`
- `RunJobNowCommand`, `PauseJobCommand`, `ResumeJobCommand`
- `ReplayDeadLetteredRunsCommand`, `QuarantineJobCommand`
- `AddJobDependencyCommand`, `RemoveJobDependencyCommand`, `GetJobDependenciesQuery`
- `GetJobExecutionLogsQuery`, `GetAllJobExecutionLogsQuery`, `GetJobsExecutionReportQuery`
- `GetJobPermissionsQuery`, `UpsertJobPermissionsCommand`

## API و سطح دسترسی
- مسیرها: `/api/v1/scheduler/*`
- `scheduler.read`, `scheduler.write`
- `scheduler.execute`, `scheduler.manage`
- `scheduler.permissions.manage`
- `scheduler.reports.read`

## وابستگی‌ها
- DataAccess برای وضعیت job/schedule/execution
- Notifications (نمونه: `NotificationDispatchProbeJobHandler`)
- Logging/metrics برای پایش
- Authorization/Audit برای کنترل دسترسی و ردگیری عملیات

## داده و نگهداشت
- اسکیما: `scheduler`
- قابلیت seed پیش‌فرض با `SchedulerOptions.SeedDefaults`
- گزینه‌های مهم عملیات:
  - `PollingIntervalSeconds`
  - `MaxDueJobsPerIteration`
  - `LockLeaseSeconds`
  - `MisfireGraceSeconds`
  - `DefaultQuarantineMinutes`
  - `NodeId`

## نکات عملیاتی
- اجرای چند نود با قفل توزیع‌شده کنترل می‌شود.
- روی failure، retry/backoff انجام می‌شود و در شکست متوالی job قرنطینه می‌شود.
- misfire policy باید با نیاز کسب‌وکاری هماهنگ تنظیم شود (`FireNow`, `Skip`, `CatchUp`).

## ریسک‌ها
- رقابت نودها بدون lock صحیح باعث اجرای تکراری job می‌شود.
- تنظیم نادرست misfire/retry می‌تواند queue lag یا اجرای burst ایجاد کند.
- مدیریت ضعیف dependency graph می‌تواند باعث بن‌بست اجرایی شود.
