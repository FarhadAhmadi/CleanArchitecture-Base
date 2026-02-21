# سند FSD ماژول زمان‌بند (Scheduler / Jobs)

تاریخ: 2026-02-21  
نسخه: 1.0 (هم‌راستا شده با معماری فعلی پروژه)  
مالک: تیم تحلیل، مستندسازی و تست

## 1) مقدمه
### هدف
ایجاد یک ماژول زمان‌بندی مطمئن، منعطف و مقیاس‌پذیر برای اجرای خودکار وظایف سازمانی (Job) بر اساس:
- Cron
- Interval
- One-time

### مسئله‌ای که حل می‌شود
- حذف وابستگی به اجرای دستی jobها
- کاهش خطای انسانی در زمان‌بندی
- کنترل‌پذیری عملیاتی بهتر (monitoring/retry/replay/reporting)

### مخاطبان
- توسعه‌دهندگان
- مدیران سیستم و DevOps
- QA
- مدیران محصول/پروژه

### دامنه
- مدیریت Job
- مدیریت Schedule
- اجرای دستی/خودکار
- مدیریت وضعیت اجرا
- لاگ و گزارش
- کنترل دسترسی
- یکپارچگی با Notification, Logging, Observability, RuleEngine (در فازهای بعد)

## 2) نیازمندی‌های کسب‌وکاری
### مدیریت Job
- ایجاد/ویرایش/فعال‌سازی/غیرفعال‌سازی Job
- نگهداری نام یکتا، توضیح، نوع Job، وضعیت
- ثبت آخرین اجرای موفق/ناموفق

### مدیریت وابستگی Jobها
- تعریف وابستگی بین jobها (Job B بعد از موفقیت Job A)
- جلوگیری از اجرای job وابسته در صورت failure والد

### زمان‌بندی پیشرفته
- Cron expressions
- Interval based scheduling
- One-time scheduling
- StartDate و EndDate

### Triggerها
- زمان‌محور (Cron/Interval/One-time)
- اجرای دستی توسط کاربر مجاز
- رویدادمحور (فاز بعدی)

### اجرای وظایف
- اجرای خودکار مطابق schedule
- اجرای دستی
- pause/resume
- ثبت نتیجه اجرا (موفق/ناموفق/لغو)

### امنیت و ACL
- دسترسی مبتنی بر permission
- ACL سطح Job (User/Role)
- ثبت audit برای عملیات مدیریتی

### گزارش و مانیتورینگ
- تاریخچه کامل اجرا
- وضعیت جاری jobها
- خروجی گزارش CSV/PDF
- اعلان خطا/تاخیر از طریق Notification

## 3) API Contract (v1)
تمام مسیرها با پیشوند `/api/v1`.

### Job Management
- `POST /api/v1/scheduler/jobs`
- `GET /api/v1/scheduler/jobs/{jobId:guid}`
- `PUT /api/v1/scheduler/jobs/{jobId:guid}`
- `DELETE /api/v1/scheduler/jobs/{jobId:guid}` (soft-disable)
- `GET /api/v1/scheduler/jobs`

### Schedule Management
- `POST /api/v1/scheduler/jobs/{jobId:guid}/schedule`
- `GET /api/v1/scheduler/jobs/{jobId:guid}/schedule`
- `PUT /api/v1/scheduler/jobs/{jobId:guid}/schedule`
- `DELETE /api/v1/scheduler/jobs/{jobId:guid}/schedule`

### Execution & Control
- `POST /api/v1/scheduler/jobs/{jobId:guid}/run`
- `POST /api/v1/scheduler/jobs/{jobId:guid}/pause`
- `POST /api/v1/scheduler/jobs/{jobId:guid}/resume`
- `GET /api/v1/scheduler/jobs/{jobId:guid}/logs`

### Permissions & Reports
- `GET /api/v1/scheduler/jobs/{jobId:guid}/permissions`
- `PUT /api/v1/scheduler/jobs/{jobId:guid}/permissions`
- `GET /api/v1/scheduler/jobs/logs`
- `GET /api/v1/scheduler/jobs/report?format=csv|pdf`

## 4) مدل دامنه پیشنهادی
- `ScheduledJob`
- `JobSchedule`
- `JobDependency`
- `JobExecution`
- `JobPermissionEntry`

### وضعیت‌های کلیدی
- JobStatus: `Active`, `Inactive`, `Paused`
- ExecutionStatus: `Scheduled`, `Running`, `Succeeded`, `Failed`, `Canceled`, `TimedOut`

## 5) Use Caseهای پیشنهادی
- `CreateJobCommand`
- `UpdateJobCommand`
- `DisableJobCommand`
- `ListJobsQuery`
- `GetJobByIdQuery`
- `UpsertJobScheduleCommand`
- `GetJobScheduleQuery`
- `DeleteJobScheduleCommand`
- `RunJobNowCommand`
- `PauseJobCommand`
- `ResumeJobCommand`
- `GetJobExecutionLogsQuery`
- `GetJobsExecutionReportQuery`
- `UpsertJobPermissionsCommand`
- `GetJobPermissionsQuery`

## 6) امنیت و سطوح دسترسی پیشنهادی
- `scheduler.read`
- `scheduler.write`
- `scheduler.execute`
- `scheduler.manage`
- `scheduler.permissions.manage`
- `scheduler.reports.read`

نکته: Audit trail برای CRUD مدیریتی اجباری است.

## 7) یکپارچگی با سایر کامپوننت‌ها
- Notification: اعلان failure/delay و SLA breach
- Logging: ثبت structured log برای شروع/پایان/خطا
- Observability: metrics (queue depth, lag, success rate, failure rate)
- RuleEngine (فاز بعد): ارزیابی قوانین قبل از اجرا
- Integration/Outbox: انتشار eventهای `JobScheduled`, `JobStarted`, `JobCompleted`, `JobFailed`

## 8) تکنولوژی و الگوی پیاده‌سازی
### گزینه پیشنهادی
- `.NET 10 / ASP.NET Core`
- زمان‌بند: `Quartz.NET` (ترجیح) یا `Hangfire` (جایگزین)
- ذخیره وضعیت سریع: Redis (اختیاری برای cache/runtime state)
- دیتابیس اصلی: SQL Server (schema مجزا)

### الگوها
- Scheduler Pattern
- Command Pattern
- Observer/Event-Driven
- Repository + Unit of Work

## 9) قیود و غیرعملکردی
- پشتیبانی حداقل 1000 job فعال
- سازگار با اجرای چند نود (distributed-safe locking)
- failover بدون اجرای تکراری ناخواسته
- دقت زمان‌بندی در سطح قابل قبول عملیاتی (ثانیه)

## 10) معیار پذیرش
- اجرای دقیق jobها مطابق schedule
- pause/resume/run دستی قابل کنترل و auditable
- گزارش کامل و قابل export
- دسترسی‌ها enforce شده و تست‌پذیر

## 11) برنامه تست
- Unit: ساخت/ویرایش/غیرفعال‌سازی Job و Schedule
- Integration: worker execution + dependency checks + retry
- Security: permission/ACL
- Performance: بار همزمان jobها و latency اجرای scheduler
- Contract: API snapshot برای سازگاری v1

## 12) ریسک‌ها و کنترل‌ها
- race condition در multi-node: استفاده از lock توزیع‌شده
- overrun jobها: timeout + cancellation token
- queue saturation: backpressure + metrics + alert
- dependency cycle: validation در زمان ثبت وابستگی

## 13) فازبندی اجرایی پیشنهادی
### فاز 1 (MVP)
- Job CRUD
- Schedule CRUD (Cron/Interval/One-time)
- Run/Pause/Resume
- Execution log

### فاز 2
- ACL per job
- CSV/PDF report
- notification on failure/delay

### فاز 3
- dependency graph
- rule-engine hook
- advanced failover tuning
