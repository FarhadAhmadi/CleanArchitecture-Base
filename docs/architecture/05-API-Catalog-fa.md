# کاتالوگ کامل API (ماژول به ماژول)

تاریخ: 2026-02-21  
مبنای استخراج: تحلیل مستقیم endpointهای `src/Web.Api/Endpoints/Modules/*`  
یادداشت: بخش `Scheduler` در این نسخه بر اساس endpointهای پیاده‌سازی‌شده به‌روزرسانی شده است.

## قواعد مسیر
- همه endpointهای ماژولی با پیشوند: `/api/v1`
- endpoint سلامت عمومی: `/health`

## سطح دسترسی
- `Permission.*`: نیازمند permission مشخص
- `Authorized User`: کاربر احراز هویت شده
- `Anonymous/Contextual`: عمومی یا مبتنی بر context خاص (مثل token-link)

---

## Audit
| Method | Path | Access |
|---|---|---|
| GET | `/api/v1/audit` | `Permissions.AuditRead` |
| GET | `/api/v1/audit/integrity` | `Permissions.AuditRead` |

## Authorization
| Method | Path | Access |
|---|---|---|
| GET | `/api/v1/authorization` | `Permissions.AuthorizationManage` |
| POST | `/api/v1/authorization/assign-permission` | `Permissions.AuthorizationManage` |
| POST | `/api/v1/authorization/assign-role` | `Permissions.AuthorizationManage` |

## Files
| Method | Path | Access |
|---|---|---|
| POST | `/api/v1/files` | `Permissions.FilesWrite` |
| POST | `/api/v1/files/validate` | `Permissions.FilesWrite` |
| POST | `/api/v1/files/scan` | `Permissions.FilesWrite` |
| GET | `/api/v1/files/{fileId:guid}` | `Permissions.FilesRead` |
| PUT | `/api/v1/files/{fileId:guid}` | `Permissions.FilesWrite` |
| DELETE | `/api/v1/files/{fileId:guid}` | `Permissions.FilesDelete` |
| GET | `/api/v1/files/{fileId:guid}/download` | `Permissions.FilesRead` |
| GET | `/api/v1/files/{fileId:guid}/stream` | `Permissions.FilesRead` |
| GET | `/api/v1/files/{fileId:guid}/link` | `Permissions.FilesShare` |
| POST | `/api/v1/files/{fileId:guid}/share` | `Permissions.FilesShare` |
| PATCH | `/api/v1/files/{fileId:guid}/move` | `Permissions.FilesWrite` |
| POST | `/api/v1/files/{fileId:guid}/tags` | `Permissions.FilesWrite` |
| GET | `/api/v1/files/tags/{tag}` | `Permissions.FilesRead` |
| GET | `/api/v1/files/search` | `Permissions.FilesRead` |
| GET | `/api/v1/files/filter` | `Permissions.FilesRead` |
| GET | `/api/v1/files/audit/{fileId:guid}` | `Permissions.FilesRead` |
| POST | `/api/v1/files/permissions/{fileId:guid}` | `Permissions.FilesPermissionsManage` |
| GET | `/api/v1/files/permissions/{fileId:guid}` | `Permissions.FilesPermissionsManage` |
| POST | `/api/v1/files/encrypt/{fileId:guid}` | `Permissions.FilesPermissionsManage` |
| POST | `/api/v1/files/decrypt/{fileId:guid}` | `Permissions.FilesPermissionsManage` |
| GET | `/api/v1/files/public/{token}` | `Anonymous/Contextual` |

## Logging
| Method | Path | Access |
|---|---|---|
| POST | `/api/v1/logging/events` | `LoggingPermissions.EventsWrite` |
| POST | `/api/v1/logging/events/bulk` | `LoggingPermissions.EventsWrite` |
| GET | `/api/v1/logging/events` | `LoggingPermissions.EventsRead` |
| GET | `/api/v1/logging/events/{eventId:guid}` | `LoggingPermissions.EventsRead` |
| GET | `/api/v1/logging/events/corrupted` | `LoggingPermissions.EventsRead` |
| DELETE | `/api/v1/logging/events/{eventId:guid}` | `LoggingPermissions.EventsDelete` |
| GET | `/api/v1/logging/schema` | `LoggingPermissions.EventsRead` |
| POST | `/api/v1/logging/validate` | `LoggingPermissions.EventsWrite` |
| POST | `/api/v1/logging/transform` | `LoggingPermissions.EventsWrite` |
| GET | `/api/v1/logging/health` | `LoggingPermissions.EventsRead` |
| POST | `/api/v1/logging/alerts/rules` | `LoggingPermissions.AlertsManage` |
| GET | `/api/v1/logging/alerts/rules` | `LoggingPermissions.AlertsManage` |
| PUT | `/api/v1/logging/alerts/rules/{id:guid}` | `LoggingPermissions.AlertsManage` |
| DELETE | `/api/v1/logging/alerts/rules/{id:guid}` | `LoggingPermissions.AlertsManage` |
| GET | `/api/v1/logging/alerts/incidents` | `LoggingPermissions.AlertsManage` |
| GET | `/api/v1/logging/alerts/incidents/{id:guid}` | `LoggingPermissions.AlertsManage` |
| GET | `/api/v1/logging/access-control` | `LoggingPermissions.AccessManage` |
| POST | `/api/v1/logging/access-control/roles` | `LoggingPermissions.AccessManage` |
| POST | `/api/v1/logging/access-control/assign` | `LoggingPermissions.AccessManage` |

## Notifications
| Method | Path | Access |
|---|---|---|
| POST | `/api/v1/notifications` | `Permissions.NotificationsWrite` |
| GET | `/api/v1/notifications` | `Permissions.NotificationsRead` |
| GET | `/api/v1/notifications/{notificationId:guid}` | `Permissions.NotificationsRead` |
| DELETE | `/api/v1/notifications/archive/{id:guid}` | `Permissions.NotificationsWrite` |
| POST | `/api/v1/notifications/{notificationId:guid}/schedule` | `Permissions.NotificationSchedulesManage` |
| GET | `/api/v1/notifications/schedules` | `Permissions.NotificationSchedulesManage` |
| DELETE | `/api/v1/notifications/schedules/{scheduleId:guid}` | `Permissions.NotificationSchedulesManage` |
| PUT | `/api/v1/notifications/{notificationId:guid}/permissions` | `Permissions.NotificationPermissionsManage` |
| GET | `/api/v1/notifications/{notificationId:guid}/permissions` | `Permissions.NotificationPermissionsManage` |
| GET | `/api/v1/notifications/reports/summary` | `Permissions.NotificationReportsRead` |
| GET | `/api/v1/notifications/reports/details` | `Permissions.NotificationReportsRead` |
| POST | `/api/v1/notifications/templates` | `Permissions.NotificationTemplatesManage` |
| PUT | `/api/v1/notifications/templates/{templateId:guid}` | `Permissions.NotificationTemplatesManage` |
| GET | `/api/v1/notification-templates` | `Permissions.NotificationTemplatesManage` |
| GET | `/api/v1/notification-templates/{templateId:guid}` | `Permissions.NotificationTemplatesManage` |
| DELETE | `/api/v1/notification-templates/{templateId:guid}` | `Permissions.NotificationTemplatesManage` |

## Observability
| Method | Path | Access |
|---|---|---|
| GET | `/api/v1/observability/metrics` | `Permissions.ObservabilityRead` |
| GET | `/api/v1/dashboard/orchestration-health` | `Permissions.ObservabilityRead` |
| GET | `/api/v1/observability/events/catalog` | `Permissions.ObservabilityRead` |
| POST | `/api/v1/observability/orchestration/replay/inbox` | `Permissions.ObservabilityManage` |
| POST | `/api/v1/observability/orchestration/replay/outbox` | `Permissions.ObservabilityManage` |

## Profiles
| Method | Path | Access |
|---|---|---|
| POST | `/api/v1/profiles/me` | `Permissions.ProfilesWrite` |
| GET | `/api/v1/profiles/me` | `Permissions.ProfilesRead` |
| PUT | `/api/v1/profiles/me/basic` | `Permissions.ProfilesWrite` |
| PATCH | `/api/v1/profiles/me/bio` | `Permissions.ProfilesWrite` |
| PATCH | `/api/v1/profiles/me/contact` | `Permissions.ProfilesWrite` |
| PATCH | `/api/v1/profiles/me/privacy` | `Permissions.ProfilesWrite` |
| PATCH | `/api/v1/profiles/me/preferences` | `Permissions.ProfilesWrite` |
| PATCH | `/api/v1/profiles/me/social-links` | `Permissions.ProfilesWrite` |
| GET | `/api/v1/profiles/me/music` | `Permissions.ProfilesRead` |
| PUT | `/api/v1/profiles/me/music` | `Permissions.ProfilesWrite` |
| POST | `/api/v1/profiles/me/interests` | `Permissions.ProfilesWrite` |
| DELETE | `/api/v1/profiles/me/interests/{interest}` | `Permissions.ProfilesWrite` |
| PUT | `/api/v1/profiles/me/avatar` | `Permissions.ProfilesWrite` |
| DELETE | `/api/v1/profiles/me/avatar` | `Permissions.ProfilesWrite` |
| GET | `/api/v1/profiles/{userId:guid}/public` | `Permissions.ProfilesPublicRead` |
| GET | `/api/v1/profiles/reports/admin` | `Permissions.ProfilesAdminRead` |

## Scheduler
| Method | Path | Access |
|---|---|---|
| POST | `/api/v1/scheduler/jobs` | `Permissions.SchedulerWrite` |
| GET | `/api/v1/scheduler/jobs` | `Permissions.SchedulerRead` |
| GET | `/api/v1/scheduler/jobs/{jobId:guid}` | `Permissions.SchedulerRead` |
| PUT | `/api/v1/scheduler/jobs/{jobId:guid}` | `Permissions.SchedulerWrite` |
| DELETE | `/api/v1/scheduler/jobs/{jobId:guid}` | `Permissions.SchedulerManage` |
| POST | `/api/v1/scheduler/jobs/{jobId:guid}/schedule` | `Permissions.SchedulerWrite` |
| GET | `/api/v1/scheduler/jobs/{jobId:guid}/schedule` | `Permissions.SchedulerRead` |
| DELETE | `/api/v1/scheduler/jobs/{jobId:guid}/schedule` | `Permissions.SchedulerManage` |
| POST | `/api/v1/scheduler/jobs/{jobId:guid}/run` | `Permissions.SchedulerExecute` |
| POST | `/api/v1/scheduler/jobs/{jobId:guid}/pause` | `Permissions.SchedulerManage` |
| POST | `/api/v1/scheduler/jobs/{jobId:guid}/resume` | `Permissions.SchedulerManage` |
| POST | `/api/v1/scheduler/jobs/{jobId:guid}/replay` | `Permissions.SchedulerManage` |
| POST | `/api/v1/scheduler/jobs/{jobId:guid}/quarantine` | `Permissions.SchedulerManage` |
| GET | `/api/v1/scheduler/jobs/{jobId:guid}/logs` | `Permissions.SchedulerRead` |
| GET | `/api/v1/scheduler/jobs/logs` | `Permissions.SchedulerReportsRead` |
| GET | `/api/v1/scheduler/jobs/report?format=csv\|pdf` | `Permissions.SchedulerReportsRead` |
| GET | `/api/v1/scheduler/jobs/{jobId:guid}/permissions` | `Permissions.SchedulerPermissionsManage` |
| PUT | `/api/v1/scheduler/jobs/{jobId:guid}/permissions` | `Permissions.SchedulerPermissionsManage` |
| POST | `/api/v1/scheduler/jobs/{jobId:guid}/dependencies` | `Permissions.SchedulerManage` |
| GET | `/api/v1/scheduler/jobs/{jobId:guid}/dependencies` | `Permissions.SchedulerRead` |
| DELETE | `/api/v1/scheduler/jobs/{jobId:guid}/dependencies/{dependsOnJobId:guid}` | `Permissions.SchedulerManage` |

## Todos
| Method | Path | Access |
|---|---|---|
| GET | `/api/v1/todos` | `Permissions.TodosRead` |
| POST | `/api/v1/todos` | `Permissions.TodosWrite` |
| GET | `/api/v1/todos/{id:guid}` | `Permissions.TodosRead` |
| PUT | `/api/v1/todos/{id:guid}/complete` | `Permissions.TodosWrite` |
| DELETE | `/api/v1/todos/{id:guid}` | `Permissions.TodosWrite` |
| POST | `/api/v1/todos/{todoId:guid}/copy` | `Permissions.TodosWrite` |

## Users
| Method | Path | Access |
|---|---|---|
| POST | `/api/v1/users/register` | `Anonymous/Contextual` |
| POST | `/api/v1/users/login` | `Anonymous/Contextual` |
| POST | `/api/v1/users/refresh` | `Anonymous/Contextual` |
| POST | `/api/v1/users/revoke` | `Anonymous/Contextual` |
| POST | `/api/v1/users/sessions/revoke-all` | `Authorized User` |
| POST | `/api/v1/users/forgot-password` | `Anonymous/Contextual` |
| POST | `/api/v1/users/reset-password` | `Anonymous/Contextual` |
| POST | `/api/v1/users/verify-email` | `Anonymous/Contextual` |
| POST | `/api/v1/users/resend-verification-code` | `Anonymous/Contextual` |
| GET | `/api/v1/users/oauth/{provider}/start` | `Anonymous/Contextual` |
| GET | `/api/v1/users/oauth/callback` | `Anonymous/Contextual` |
| GET | `/api/v1/users/me` | `Authorized User` |
| GET | `/api/v1/users/{userId}` | `Permissions.UsersAccess` |

## Core/Operational
| Method | Path | Access |
|---|---|---|
| GET | `/health` | عمومی |

---

## توضیح تکمیلی
این سند برای inventory و governance است. برای توضیح معماری و دلیل وجود هر endpoint به اسناد زیر مراجعه کنید:
- `docs/architecture/01-System-Architecture-fa.md`
- `docs/architecture/03-Module-Catalog-fa.md`
