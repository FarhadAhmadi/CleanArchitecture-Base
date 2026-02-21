# کاتالوگ ماژول ها و فیچرها (قالب استاندارد)

تاریخ: 2026-02-21  
مبنای تحلیل: commit `6db4c27`

## الگوی استاندارد هر ماژول
- هدف کسب وکاری
- قابلیت های کلیدی
- مدل دامنه
- use caseها
- سطح دسترسی
- وابستگی ها
- وضعیت کیفیت و تست
- ریسک های اجرایی

---

## M1) Users/Auth

### هدف
مدیریت چرخه عمر هویت کاربر: ثبت نام، ورود، refresh/revoke، تایید ایمیل، بازیابی رمز، OAuth.

### قابلیت ها
- Register + default role assignment
- Login با lockout + email-confirmation gate
- Refresh token rotation + reuse detection
- Revoke token و revoke-all-sessions
- Verify email / resend verification code
- Forgot/reset password با کد یکبارمصرف
- OAuth start/callback (Google/Meta)

### مدل دامنه
- `User`
- `RefreshToken`
- `UserExternalLogin`
- `UserRegisteredDomainEvent`

### use caseهای اصلی
- `RegisterUserCommand`
- `LoginUserCommand`
- `RefreshTokenCommand`
- `RevokeRefreshTokenCommand`
- `VerifyEmailCommand`
- `RequestPasswordResetCommand`
- `ConfirmPasswordResetCommand`
- `CompleteOAuthCommand`

### سطح دسترسی
- عمدتا public/anonymous برای auth flow
- `GET /users/me` نیازمند کاربر احراز هویت شده
- `GET /users/{id}` نیازمند `users:access`

### وابستگی ها
- Identity Core
- AuditTrailService
- Notification module (ارسال کد تایید/ریست)

### تست های مرتبط
- `tests/ArchitectureTests/Modules/Users/*`
- `tests/ArchitectureTests/Security/AuthFlowTests.cs`

### ریسک ها
- وابستگی به کانال اعلان برای completion برخی flowها
- نیاز به مدیریت دقیق تنظیمات امنیتی JWT

---

## M2) Authorization

### هدف
مدیریت RBAC + Permission-based authorization.

### قابلیت ها
- تخصیص role به user
- تخصیص permission به role
- دریافت نمای کامل access-control matrix
- policy generation پویا بر اساس permission code

### مدل دامنه
- `Role`, `Permission`, `UserRole`, `RolePermission`, `UserPermission`

### use caseهای اصلی
- `AssignRoleToUserCommand`
- `AssignPermissionToRoleCommand`
- `GetAccessControlQuery`

### سطح دسترسی
همه endpointها نیازمند `authorization:manage`

### وابستگی ها
- Permission cache/versioning (Redis/Memory)
- Audit module (ثبت عملیات مدیریتی)

### تست های مرتبط
- Auth flow tests (احترام به permissions)

### ریسک ها
- رشد ماتریس نقش/مجوز در سازمان های بزرگ

---

## M3) Todos

### هدف
مدیریت task شخصی کاربر.

### قابلیت ها
- ایجاد، مشاهده لیستی، مشاهده موردی
- تکمیل، حذف، کپی
- paging/search/sort

### مدل دامنه
- `TodoItem`
- `Priority`
- domain events (`Created`, `Completed`, `Deleted`)

### use caseهای اصلی
- `CreateTodoCommand`
- `GetTodosQuery`
- `GetTodoByIdQuery`
- `CompleteTodoCommand`
- `DeleteTodoCommand`
- `CopyTodoCommand`

### سطح دسترسی
- خواندن: `todos:read`
- نوشتن: `todos:write`

### وابستگی ها
- User context
- EF data access

### تست های مرتبط
- auth permission tests برای endpointهای todos

### ریسک ها
- کم (ماژول ساده، coupling محدود)

---

## M4) Profiles

### هدف
مدیریت پروفایل کاربر با ابعاد عمومی/خصوصی و گزارش مدیریتی.

### قابلیت ها
- create/get/update پروفایل شخصی
- مدیریت bio/contact/privacy/preferences/social/music/interests/avatar
- دریافت public profile
- گزارش مدیریتی پروفایل ها با فیلترهای تحلیلی
- محاسبه Profile Completeness

### مدل دامنه
- `UserProfile`
- `UserProfileChangedDomainEvent`

### use caseهای اصلی
- `CreateMyProfileCommand`
- `GetMyProfileQuery`
- `UpdateMyProfileBasic/Bio/Contact/Privacy/Preferences/SocialLinks/...`
- `GetPublicProfileQuery`
- `GetProfilesAdminReportQuery`

### سطح دسترسی
- read: `profiles.read`
- write: `profiles.write`
- public read: `profiles.public.read`
- admin report: `profiles.admin.read`

### وابستگی ها
- Files (avatar/music file ids)
- Logging + Notifications (روی DomainEvent تغییر پروفایل)

### تست های مرتبط
- `tests/ArchitectureTests/Modules/Profiles/*`

### ریسک ها
- رشد قوانین privacy و نیاز به governance دقیق فیلدها

---

## M5) Files

### هدف
سرویس فایل سازمانی: ذخیره، جستجو، ACL، audit، لینک امن عمومی.

### قابلیت ها
- upload/validate/scan
- metadata, download, stream
- secure link + public token access
- move, tagging, search/filter
- file ACL و permission entries
- encryption flag management
- file access audit trail

### مدل دامنه
- `FileAsset`
- `FileTag`
- `FilePermissionEntry`
- `FileAccessAudit`

### use caseهای اصلی
- `UploadFileCommand`, `ValidateFileCommand`, `ScanFileCommand`
- `GetFileMetadataQuery`, `DownloadFileQuery`, `StreamFileQuery`
- `GetSecureFileLinkQuery`, `GetPublicFileByLinkQuery`
- `SearchFilesQuery`, `FilterFilesQuery`, `SearchFilesByTagQuery`
- `UpsertFilePermissionCommand`, `GetFilePermissionsQuery`

### سطح دسترسی
- `files.read`, `files.write`, `files.delete`, `files.share`, `files.permissions.manage`
- endpoint عمومی لینک فایل بدون JWT ولی با token + rate limit + audit

### وابستگی ها
- MinIO object storage
- ClamAV scanner (optional)
- Logging module برای public link audits

### تست های مرتبط
- `tests/ArchitectureTests/Security/PublicFileLinkSecurityTests.cs`

### ریسک ها
- مدیریت کلید signing لینک عمومی و expiry policy
- نیاز به فعال سازی scanner واقعی در production

---

## M6) Notifications

### هدف
ارسال اعلان چندکاناله با template/schedule/ACL/reporting.

### قابلیت ها
- create/list/get/archive notification
- template CRUD + revision history
- schedule و cancel schedule
- ACL per-notification (User/Role)
- summary/details reports
- dispatch worker با retry/backoff

### مدل دامنه
- `NotificationMessage`
- `NotificationTemplate` + `NotificationTemplateRevision`
- `NotificationSchedule`
- `NotificationPermissionEntry`
- `NotificationDeliveryAttempt`

### use caseهای اصلی
- `CreateNotificationCommand`
- `ScheduleNotificationCommand`
- `Create/Update/DeleteNotificationTemplateCommand`
- `ListNotificationsQuery`, `GetNotificationQuery`
- `GetNotificationSummaryReportQuery`, `GetNotificationDetailsReportQuery`

### سطح دسترسی
- `notifications.read`, `notifications.write`
- `notifications.templates.manage`
- `notifications.schedules.manage`
- `notifications.permissions.manage`
- `notifications.reports.read`

### وابستگی ها
- channel senders: Email/SMS/Slack/Teams/Push/InApp
- recipient encryption/protection

### تست های مرتبط
- `tests/ArchitectureTests/Modules/Notifications/*`

### ریسک ها
- وابستگی به credential/provider های بیرونی
- کلید encryption recipient باید امن مدیریت شود

---

## M7) Logging

### هدف
پلتفرم لاگ داخلی با schema validation، integrity، alerts و access-control.

### قابلیت ها
- ingest single/bulk event
- schema/validate/transform
- query events + corrupted events
- soft delete event
- alert rule CRUD
- alert incident query
- access-control for logging roles
- ingestion retry queue + alert dispatch queue

### مدل دامنه
- `LogEvent`
- `AlertRule`
- `AlertIncident`
- `AlertIncidentCreatedDomainEvent`

### use caseهای اصلی
- `IngestSingleLogCommand`, `IngestBulkLogsCommand`
- `GetLogEventsQuery`, `GetCorruptedLogEventsQuery`
- `CreateAlertRuleCommand`, `UpdateAlertRuleCommand`, `DeleteAlertRuleCommand`
- `GetAlertIncidentsQuery`
- `AssignLoggingAccessCommand`

### سطح دسترسی
- `logging.events.write/read/delete`
- `logging.alerts.manage`
- `logging.access.manage`

### وابستگی ها
- Notification module (برای dispatch incident به admin)
- Internal queues + workers + retry

### تست های مرتبط
- security hardening tests و endpoint access checks

### ریسک ها
- queue saturation در بار بسیار بالا (نیاز به ظرفیت سنجی)

---

## M8) Audit

### هدف
ثبت trail تغییرات حساس به صورت tamper-evident.

### قابلیت ها
- ثبت رکورد با hash payload
- checksum chain با previous checksum
- integrity report و tamper flagging
- query paged audit trail

### مدل دامنه
- `AuditEntry`

### use caseهای اصلی
- `GetAuditEntriesQuery`
- `GetAuditIntegrityReportQuery`

### سطح دسترسی
- `audit.read`

### وابستگی ها
- توسط ماژول های دیگر برای ثبت عملیات حساس مصرف می شود

### تست های مرتبط
- `tests/ArchitectureTests/Modules/Audit/*`

### ریسک ها
- حجم داده در بلندمدت (نیاز به policy آرشیو)

---

## M9) Observability

### هدف
ارائه KPI عملیاتی، سلامت orchestration و کنترل replay.

### قابلیت ها
- operational metrics snapshot + SLO evaluation
- orchestration health snapshot
- event contract catalog
- replay failed outbox/inbox

### use caseهای اصلی
- `GetOperationalMetricsQuery`
- `GetOrchestrationHealthQuery`
- `GetEventCatalogQuery`
- `ReplayFailedOutboxCommand`
- `ReplayFailedInboxCommand`

### سطح دسترسی
- read: `observability.read`
- manage/replay: `observability.manage`

### وابستگی ها
- Logging, Integration, Notifications داده های سلامت

### تست های مرتبط
- endpoint access در auth/security tests

### ریسک ها
- replay اشتباه بدون governance عملیاتی (نیاز به SOP)

---

## M10) پلتفرم مشترک (Cross-cutting)

### اجزا
- Core module: time provider + domain event dispatcher
- DataAccess module: read/write DbContext + SQL retry
- Caching module: Redis/Memory cache + permission cache versioning
- Integration module: outbox/inbox + RabbitMQ workers
- HealthChecks module

### نقش
پایه فنی مشترک برای همه ماژول ها با هدف کاهش تکرار و افزایش پایداری.

---

## جمع بندی
مرزبندی ماژولی واقعی در کد اجرا شده است و قابلیت ها به صورت endpoint/handler/domain قابل ردیابی هستند.  
برای مرجع endpoint کامل به `docs/architecture/05-API-Catalog-fa.md` مراجعه شود.
