# ماژول Logging

تاریخ به‌روزرسانی: 2026-02-21

## هدف
پلتفرم ingest و تحلیل لاگ با validation، alerting و access-control.

## ترتیب IOrderedEndpoint
این ماژول از `IOrderedEndpoint` استفاده نمی‌کند. مسیرها در `MapLoggingEndpoints` با ترتیب زیر register می‌شوند:
1) ingest/queries/schema/health
2) alert rules/incidents
3) access-control

## کاتالوگ کامل Endpointها
| Method | Path | دسترسی | دلیل وجود | ورودی‌ها |
|---|---|---|---|---|
| POST | `/api/v1/logging/events` | `logging.events.write` | ingest تک‌رخداد | Body: `IngestLogRequest` + Header: `Idempotency-Key` |
| POST | `/api/v1/logging/events/bulk` | `logging.events.write` | ingest دسته‌ای | Body: `BulkIngestRequest{ events[] }` + Header: `Idempotency-Key` |
| GET | `/api/v1/logging/events` | `logging.events.read` | جستجو/صفحه‌بندی رخدادها | Query: `GetLogEventsRequest` |
| GET | `/api/v1/logging/events/{eventId:guid}` | `logging.events.read` | دریافت رخداد | Path: `eventId`, Query: `recalculateIntegrity` |
| GET | `/api/v1/logging/events/corrupted` | `logging.events.read` | رخدادهای خراب/مشکوک | Query: `recalculate` |
| DELETE | `/api/v1/logging/events/{eventId:guid}` | `logging.events.delete` | حذف نرم رخداد | Path: `eventId` |
| GET | `/api/v1/logging/schema` | `logging.events.read` | دریافت schema مجاز | - |
| POST | `/api/v1/logging/validate` | `logging.events.write` | اعتبارسنجی payload قبل ingest | Body: `IngestLogRequest` |
| POST | `/api/v1/logging/transform` | `logging.events.write` | نرمال‌سازی payload | Body: `IngestLogRequest` |
| GET | `/api/v1/logging/health` | `logging.events.read` | سلامت pipeline لاگ | - |
| POST | `/api/v1/logging/alerts/rules` | `logging.alerts.manage` | ایجاد rule هشدار | Body: `CreateAlertRuleRequest` |
| GET | `/api/v1/logging/alerts/rules` | `logging.alerts.manage` | لیست ruleها | - |
| PUT | `/api/v1/logging/alerts/rules/{id:guid}` | `logging.alerts.manage` | بروزرسانی rule | Path: `id`, Body: `UpdateAlertRuleRequest` |
| DELETE | `/api/v1/logging/alerts/rules/{id:guid}` | `logging.alerts.manage` | حذف rule | Path: `id` |
| GET | `/api/v1/logging/alerts/incidents` | `logging.alerts.manage` | لیست incidentها | Query: `page`, `pageSize`, `status` |
| GET | `/api/v1/logging/alerts/incidents/{id:guid}` | `logging.alerts.manage` | جزئیات incident | Path: `id` |
| GET | `/api/v1/logging/access-control` | `logging.access.manage` | مشاهده نقش/مجوز logging | - |
| POST | `/api/v1/logging/access-control/roles` | `logging.access.manage` | ایجاد نقش logging | Body: `CreateRoleRequest{ roleName }` |
| POST | `/api/v1/logging/access-control/assign` | `logging.access.manage` | تخصیص permission logging | Body: `AssignAccessRequest{ roleName, permissionCode }` |

## ورودی‌های اصلی
### `IngestLogRequest`
- `timestampUtc`, `level`, `message`
- `sourceService`, `sourceModule`
- `traceId`, `requestId`, `tenantId`
- `actorType`, `actorId`, `outcome`
- `sessionId`, `ip`, `userAgent`, `deviceInfo`
- `httpMethod`, `httpPath`, `httpStatusCode`
- `errorCode`, `errorStackHash`, `tags[]`, `payloadJson`

### `GetLogEventsRequest`
- paging/sorting: `page/pageIndex`, `pageSize`, `sortBy`, `sortOrder`
- filters: `level`, `from`, `to`, `actorId`, `service`, `module`, `traceId`, `outcome`, `text`
- `recalculateIntegrity`

## نکات طراحی مهم
- تمام مسیرها در `RouteGroup(/api/v1/logging)` تعریف می‌شوند.
- endpoint filterهای ثبت اجرا/sanitization روی کل group اعمال شده‌اند.
- idempotency key از header خوانده می‌شود تا ingest تکراری کنترل شود.

## وابستگی‌ها
- schema `logging`
- integration با Notifications (incident dispatch)
- pipeline queue/retry داخلی

## سناریوهای خطا
- ingest payload نامعتبر
- queue backlog و افزایش latency
- rule misconfiguration و alert noise
