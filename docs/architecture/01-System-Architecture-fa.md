# Ø³Ù†Ø¯ Ù…Ø¹Ù…Ø§Ø±ÛŒ Ø³ÛŒØ³ØªÙ… (Ù†Ø³Ø®Ù‡ Ø§Ø¬Ø±Ø§ÛŒÛŒ)

ØªØ§Ø±ÛŒØ®: 2026-02-21  
Ù…Ø¨Ù†Ø§ÛŒ ØªØ­Ù„ÛŒÙ„: commit `6db4c27`

## 1) Ø®Ù„Ø§ØµÙ‡ Ù…Ø¯ÛŒØ±ÛŒØªÛŒ
Ø§ÛŒÙ† Ù…Ø­ØµÙˆÙ„ ÛŒÚ© **Modular Monolith** Ù…Ø¨ØªÙ†ÛŒ Ø¨Ø± **Clean Architecture** Ø§Ø³Øª Ú©Ù‡ Ø¯Ø± ÛŒÚ© Ø³Ø±ÙˆÛŒØ³ Ø§ØµÙ„ÛŒ (`Web.Api`) Ø§Ø¬Ø±Ø§ Ù…ÛŒ Ø´ÙˆØ¯ Ùˆ Ù…Ø§Ú˜ÙˆÙ„ Ù‡Ø§ÛŒ Ú©Ø³Ø¨ ÙˆÚ©Ø§Ø±ÛŒ Ø±Ø§ Ø¨Ø§ Ù…Ø±Ø²Ù‡Ø§ÛŒ Ù…Ø´Ø®Øµ Ø¯Ø± Ù„Ø§ÛŒÙ‡ Ù‡Ø§ÛŒ Domain/Application/Infrastructure Ù†Ú¯Ù‡ Ù…ÛŒ Ø¯Ø§Ø±Ø¯.

Ø³ÛŒØ³ØªÙ… Ø¨Ø±Ø§ÛŒ Ù…Ø±Ø­Ù„Ù‡ Ø±Ø´Ø¯ Ù…Ø­ØµÙˆÙ„ Ø·Ø±Ø§Ø­ÛŒ Ø´Ø¯Ù‡ Ø§Ø³Øª:
- Ø³Ø±Ø¹Øª ØªÙˆØ³Ø¹Ù‡ Ø¨Ø§Ù„Ø§ (ÛŒÚ© deployable)
- Ú©Ù†ØªØ±Ù„ Ø§Ù…Ù†ÛŒØª Ùˆ Ù…Ø¬ÙˆØ²Ø¯Ù‡ÛŒ Ø¯Ù‚ÛŒÙ‚ (Role + Permission)
- Ù‚Ø§Ø¨Ù„ÛŒØª Ø§Ø·Ù…ÛŒÙ†Ø§Ù† Ø¹Ù…Ù„ÛŒØ§ØªÛŒ (Outbox/Inbox + retry + health)
- Ù…Ø´Ø§Ù‡Ø¯Ù‡ Ù¾Ø°ÛŒØ±ÛŒ Ø¹Ù…Ù„ÛŒØ§ØªÛŒ (metrics + logging + alerting)

## 2) Ø³Ø¨Ú© Ù…Ø¹Ù…Ø§Ø±ÛŒ Ùˆ Ø¯Ù„ÛŒÙ„ Ø§Ù†ØªØ®Ø§Ø¨

### Ø³Ø¨Ú© Ø§ØµÙ„ÛŒ
- Modular Monolith
- Clean Architecture
- CQRS Ø³Ø¨Ú© (Command/Query handlers)
- Domain Events + Outbox/Inbox

### Ú†Ø±Ø§ Ø§ÛŒÙ† Ø§Ù†ØªØ®Ø§Ø¨ Ù…Ù†Ø·Ù‚ÛŒ Ø§Ø³Øª
- Ù†Ø³Ø¨Øª Ø¨Ù‡ MicroserviceØŒ Ù‡Ø²ÛŒÙ†Ù‡ Ø¹Ù…Ù„ÛŒØ§ØªÛŒ Ù¾Ø§ÛŒÛŒÙ† ØªØ± Ùˆ ØªØ­ÙˆÛŒÙ„ Ø³Ø±ÛŒØ¹ ØªØ± Ø¯Ø§Ø±Ø¯.
- Ù†Ø³Ø¨Øª Ø¨Ù‡ Monolith Ø³Ù†ØªÛŒØŒ Ù…Ø±Ø²Ø¨Ù†Ø¯ÛŒ Ù…Ø§Ú˜ÙˆÙ„ Ù‡Ø§ Ùˆ Ø§Ù…Ù†ÛŒØª/Ù¾Ø§ÛŒØ¯Ø§Ø±ÛŒ Ø¨Ø³ÛŒØ§Ø± Ø¨Ù‡ØªØ± Ø§Ø³Øª.
- Ø¨Ø±Ø§ÛŒ ØªØ¨Ø¯ÛŒÙ„ ØªØ¯Ø±ÛŒØ¬ÛŒ Ø¨Ù‡ Ø³Ø±ÙˆÛŒØ³ Ù‡Ø§ÛŒ Ù…Ø³ØªÙ‚Ù„ Ø¯Ø± Ø¢ÛŒÙ†Ø¯Ù‡ØŒ Ù…Ø³ÛŒØ± Ù…Ù‡Ø§Ø¬Ø±Øª Ù…Ø´Ø®Øµ Ù†Ú¯Ù‡ Ø¯Ø§Ø´ØªÙ‡ Ø´Ø¯Ù‡ Ø§Ø³Øª.

## 3) Ø³Ø§Ø®ØªØ§Ø± Ù„Ø§ÛŒÙ‡ Ø§ÛŒ Ø§Ø¬Ø±Ø§ Ø´Ø¯Ù‡ Ø¯Ø± Ú©Ø¯
- Domain: `src/Domain/Modules/*`
- Application: `src/Application/Modules/*` Ùˆ `src/Application/Abstractions/*`
- Infrastructure: `src/Platform/*` Ùˆ `src/Platform/Modules/*`
- API/Presentation: `src/Host/*`
- SharedKernel: `src/SharedKernel/*`

Ù‚ÙˆØ§Ø¹Ø¯ Ù…Ø¹Ù…Ø§Ø±ÛŒ Ø¨Ø§ ØªØ³Øª enforce Ø´Ø¯Ù‡ Ø§Ù†Ø¯:
- Domain Ø¨Ù‡ Application/Infrastructure/Web ÙˆØ§Ø¨Ø³ØªÙ‡ Ù†ÛŒØ³Øª.
- Application Ø¨Ù‡ Infrastructure/Web ÙˆØ§Ø¨Ø³ØªÙ‡ Ù†ÛŒØ³Øª.
- Infrastructure Ø¨Ù‡ Web ÙˆØ§Ø¨Ø³ØªÙ‡ Ù†ÛŒØ³Øª.

Ù…Ø±Ø¬Ø¹: `tests/ArchitectureTests/Layers/LayerTests.cs`

## 4) Composition Root Ùˆ Ù…Ø§Ú˜ÙˆÙ„ Ø¨Ù†Ø¯ÛŒ
Composition Root Ø§Ø² `Program.cs` Ùˆ `Infrastructure.DependencyInjection` Ø§Ù†Ø¬Ø§Ù… Ù…ÛŒ Ø´ÙˆØ¯.

Ø²Ù†Ø¬ÛŒØ±Ù‡ Ø«Ø¨Øª Ø³Ø±ÙˆÛŒØ³:
- `AddApplication()`
- `AddPresentation(configuration)`
- `AddInfrastructure(configuration)`

Ù…Ø§Ú˜ÙˆÙ„ Ù‡Ø§ÛŒ Infrastructure Ø«Ø¨Øª Ø´Ø¯Ù‡:
- Core
- DataAccess
- Caching
- Integration
- HealthChecks
- Logging
- Users
- Auth
- Authorization
- Audit
- Monitoring
- Files
- Notifications
- Scheduler

Ù…Ø±Ø¬Ø¹: `src/Host/Program.cs`, `src/Platform/DependencyInjection.cs`

## 5) Ù…Ø§Ú˜ÙˆÙ„ Ù‡Ø§ÛŒ Ú©Ø³Ø¨ ÙˆÚ©Ø§Ø±ÛŒ ÙØ¹Ø§Ù„
- Users/Auth
- Authorization
- Todos
- Profiles
- Files
- Notifications
- Logging
- Audit
- Observability
- Scheduler (job orchestration)

## 5.1) ÙˆØ¶Ø¹ÛŒØª ÙØ¹Ù„ÛŒ Scheduler
- endpointÙ‡Ø§ÛŒ scheduler Ø¯Ø± `src/Host/Endpoints/Modules/Scheduler/*` ÙØ¹Ø§Ù„ Ù‡Ø³ØªÙ†Ø¯.
- worker Ø²Ù…Ø§Ù†â€ŒØ¨Ù†Ø¯ Ø¯Ø± `src/Platform/Modules/Scheduler/Infrastructure/Scheduling/SchedulerWorker.cs` ÙØ¹Ø§Ù„ Ø§Ø³Øª.
- Ù‚Ø§Ø¨Ù„ÛŒØªâ€ŒÙ‡Ø§ÛŒ runtime Ø´Ø§Ù…Ù„ retry, misfire policy, replay, quarantine Ùˆ dependency check Ø§Ø³Øª.

Ø¬Ø²Ø¦ÛŒØ§Øª Ù‡Ø± Ù…Ø§Ú˜ÙˆÙ„ Ø¯Ø± `docs/architecture/03-Module-Catalog-fa.md` Ø¢Ù…Ø¯Ù‡ Ø§Ø³Øª.

## 6) Ù…Ø¹Ù…Ø§Ø±ÛŒ Ø¯Ø§Ø¯Ù‡ Ùˆ Ù…Ø±Ø²Ù‡Ø§ÛŒ Schema
Ø³ÛŒØ³ØªÙ… ÛŒÚ© Ø¯ÛŒØªØ§Ø¨ÛŒØ³ Ø§ØµÙ„ÛŒ SQL Server Ø¯Ø§Ø±Ø¯ Ø§Ù…Ø§ Ø¨Ø§ schema-based modularization.

| Schema | Ù‡Ø¯Ù |
|---|---|
| `users` | Ø¯Ø§Ø¯Ù‡ Ù‡Ø§ÛŒ Ú©Ø§Ø±Ø¨Ø± Ùˆ external login |
| `auth` | RBAC/Permissions/RefreshTokens/Identity tables |
| `todos` | Todo domain |
| `profiles` | Profile domain |
| `files` | ÙØ§ÛŒÙ„ Ù‡Ø§ØŒ ACL ÙØ§ÛŒÙ„ØŒ audit ÙØ§ÛŒÙ„ |
| `notifications` | Ø§Ø¹Ù„Ø§Ù†ØŒ templateØŒ scheduleØŒ attempts |
| `logging` | Ø±Ø®Ø¯Ø§Ø¯Ù‡Ø§ÛŒ Ù„Ø§Ú¯ØŒ ruleØŒ incident |
| `audit` | audit trail tamper-evident |
| `integration` | outbox/inbox |
| `dbo` | Ù¾ÛŒØ´ ÙØ±Ø¶ EF Ùˆ metadata |

Ù…Ø±Ø¬Ø¹: `src/Platform/Database/Schemas.cs` Ùˆ `src/Platform/Database/Configurations/*`

## 7) Ø§Ù…Ù†ÛŒØª Ùˆ Ú©Ù†ØªØ±Ù„ Ø¯Ø³ØªØ±Ø³ÛŒ

### Authentication
- JWT Bearer
- Refresh Token rotation + revoke + reuse detection
- Lockout policy
- OAuth (Google/Meta)

Ù…Ø±Ø¬Ø¹: `src/Platform/Modules/AuthModule.cs`, `src/Modules/Users/Application/*`

### Authorization
- Dynamic policy-based permission (`RequireAuthorization(permissionCode)`)
- Role + Permission model
- Permission cache versioning Ø±ÙˆÛŒ Redis/Memory

Ù…Ø±Ø¬Ø¹: `src/Modules/Authorization/Infrastructure/Infrastructure/Authorization/*`

### Hardening
- Security headers (CSP, X-Frame-Options, ...)
- Ø­Ø°Ù `Server` header
- Request hardening (header count/size/content-type)
- Rate limiting Ø³Ø±Ø§Ø³Ø±ÛŒ + rate limit ÙˆÛŒÚ˜Ù‡ Ù„ÛŒÙ†Ú© Ø¹Ù…ÙˆÙ…ÛŒ ÙØ§ÛŒÙ„
- Global exception handling Ø¨Ø¯ÙˆÙ† Ø§ÙØ´Ø§ÛŒ Ø§Ø·Ù„Ø§Ø¹Ø§Øª Ø­Ø³Ø§Ø³

Ù…Ø±Ø¬Ø¹: `src/Host/Middleware/*`, `src/Host/DependencyInjection.cs`, `src/Host/Infrastructure/GlobalExceptionHandler.cs`

### Audit
- Audit trail chain (checksum + previous checksum)
- Integrity report endpoint

Ù…Ø±Ø¬Ø¹: `src/Platform/Modules/Audit/Infrastructure/Auditing/AuditTrailService.cs`, `src/Application/Modules/Audit/*`

## 8) Ù‚Ø§Ø¨Ù„ÛŒØª Ø§Ø·Ù…ÛŒÙ†Ø§Ù† Ùˆ Ù¾Ø§ÛŒØ¯Ø§Ø±ÛŒ

### Reliable messaging
- Outbox Ø¨Ø±Ø§ÛŒ Ø§Ù†ØªØ´Ø§Ø± eventÙ‡Ø§ÛŒ Ø¯Ø§Ù…Ù†Ù‡
- Inbox Ø¨Ø±Ø§ÛŒ Ù…ØµØ±Ù idempotent Ù¾ÛŒØ§Ù… Ù‡Ø§
- RabbitMQ publisher/consumer workers

Ù…Ø±Ø¬Ø¹: `src/Platform/Integration/*`, `src/Platform/Messaging/*`

### Retry/Resilience
- SQL retry (`EnableRetryOnFailure`)
- Polly pipelines Ø¯Ø± background workers
- HTTP resilience handler

Ù…Ø±Ø¬Ø¹: `src/Platform/Modules/DataAccessModule.cs`, `src/Platform/*Worker.cs`, `src/Host/DependencyInjection.cs`

### Health & Ops
- `/health`
- Outbox health check
- Operational metrics + orchestration health + replay endpoints
- Operational alert worker (Webhook/PagerDuty)

Ù…Ø±Ø¬Ø¹: `src/Platform/Modules/HealthChecksModule.cs`, `src/Application/Modules/Observability/*`, `src/Host/Infrastructure/OperationalAlertWorker.cs`

## 9) Observability
- Structured logging Ø¨Ø§ Serilog
- Sink Ø¨Ù‡ Seq ÛŒØ§ Elasticsearch
- OpenTelemetry tracing + metrics (OTLP optional)
- Logging platform Ø¯Ø§Ø®Ù„ÛŒ (ingest, schema, validation, alerts)
- Logging Decorator Ø¯Ø± Application Ø¨Ø±Ø§ÛŒ Ø«Ø¨Øª Ø´Ø±ÙˆØ¹/Ù¾Ø§ÛŒØ§Ù†/Ø®Ø·Ø§ Ùˆ slow operation ÙØ¹Ø§Ù„ Ø§Ø³Øª.
- Domain event dispatch logging Ø¨Ø±Ø§ÛŒ Ø´Ù…Ø§Ø±Ø´ handlerÙ‡Ø§ Ùˆ Ø±ÙˆÛŒØ¯Ø§Ø¯Ù‡Ø§ÛŒ Ø¨Ø¯ÙˆÙ† handler ÙØ¹Ø§Ù„ Ø§Ø³Øª.

Ù…Ø±Ø¬Ø¹: `src/Host/Program.cs`, `src/Host/DependencyInjection.cs`, `src/Platform/Modules/Logging/*`

## 10) Ø§Ø³ØªÙ‚Ø±Ø§Ø± Ùˆ Ø¹Ù…Ù„ÛŒØ§Øª

### Local/dev
- Docker Compose stack: SQL Server, RabbitMQ, Redis, MinIO, Seq, Elasticsearch, Kibana, Logstash, WebApi
- Ø§Ø³Ú©Ø±ÛŒÙ¾Øª ÛŒÚ©Ù¾Ø§Ø±Ú†Ù‡ Ø§Ø¬Ø±Ø§: `scripts/dev/dev-stack.ps1`

### CI/CD
- CI: restore/build/format/test/publish + migration artifact + image scan/push
- CD: AKS deploy + blue/green + smoke test + rollback on failure
- Security: CodeQL + NuGet vuln scan + Gitleaks
- DR drill: Ø²Ù…Ø§Ù† Ø¨Ù†Ø¯ÛŒ Ù‡ÙØªÚ¯ÛŒ
- Infra: Terraform plan/validate

Ù…Ø±Ø¬Ø¹: `.github/workflows/*.yml`, `deploy/k8s/*`, `infra/terraform/*`

## 11) Ú©Ù†ØªØ±Ù„ Ú©ÛŒÙÛŒØª Ùˆ Ø´ÙˆØ§Ù‡Ø¯ ÙÙ†ÛŒ
- Architecture tests
- Security tests
- Module integration tests (Users, Profiles, Notifications, Audit)
- OpenAPI snapshot contract tests
- k6 load/soak assets
- ØªØ³Øª Ø±ÙØªØ§Ø± permission Ø¨Ø±Ø§ÛŒ endpointÙ‡Ø§ÛŒ scheduler

Ù…Ø±Ø¬Ø¹: `tests/ArchitectureTests/*`, `tests/ContractTests/*`, `tests/Performance/*`

## 12) Ù…Ø²Ø§ÛŒØ§ØŒ Ù‡Ø²ÛŒÙ†Ù‡ Ù‡Ø§ Ùˆ trade-off
| Ù…Ø­ÙˆØ± | Ù…Ø²ÛŒØª | Ù‡Ø²ÛŒÙ†Ù‡/Ø±ÛŒØ³Ú© | Ú©Ù†ØªØ±Ù„ ÙØ¹Ù„ÛŒ |
|---|---|---|---|
| Modular Monolith | Ø³Ø±Ø¹Øª ØªØ­ÙˆÛŒÙ„ Ø¨Ø§Ù„Ø§ + Ø³Ø§Ø¯Ú¯ÛŒ Ø¹Ù…Ù„ÛŒØ§Øª | Ø§Ø­ØªÙ…Ø§Ù„ coupling ØªØ¯Ø±ÛŒØ¬ÛŒ | Ù„Ø§ÛŒÙ‡ Ø¨Ù†Ø¯ÛŒ + ØªØ³Øª Ù…Ø¹Ù…Ø§Ø±ÛŒ |
| RBAC+Permission | Ú©Ù†ØªØ±Ù„ Ø¯Ø³ØªØ±Ø³ÛŒ Ø¯Ù‚ÛŒÙ‚ | Ù¾ÛŒÚ†ÛŒØ¯Ú¯ÛŒ Ù…Ø¯ÛŒØ±ÛŒØª Ù†Ù‚Ø´/Ù…Ø¬ÙˆØ² | seeding + endpoint permission gating |
| Outbox/Inbox | Ú©Ø§Ù‡Ø´ Ø®Ø·Ø§ Ø¯Ø± ØªØ¹Ø§Ù…Ù„Ø§Øª async | Ù†Ú¯Ù‡Ø¯Ø§Ø±ÛŒ worker Ùˆ queue | health + replay + retry |
| Logging Platform Ø¯Ø§Ø®Ù„ÛŒ | Ù‚Ø§Ø¨Ù„ÛŒØª ØªØ­Ù„ÛŒÙ„/alert Ø¹Ù…ÛŒÙ‚ | Ù‡Ø²ÛŒÙ†Ù‡ Ù†Ú¯Ù‡Ø¯Ø§Ø±ÛŒ Ù†Ø³Ø¨Øª Ø¨Ù‡ SaaS | schema + ACL + queue health |
| File/Public Link | ØªØ¬Ø±Ø¨Ù‡ Ø§Ø´ØªØ±Ø§Ú© Ø³Ø±ÛŒØ¹ | Ø±ÛŒØ³Ú© misuse Ù„ÛŒÙ†Ú© | signed token + rate limit + audit log |
| Notification Ú†Ù†Ø¯Ú©Ø§Ù†Ø§Ù„Ù‡ | Ø§Ù†Ø¹Ø·Ø§Ù Ú©Ø§Ù†Ø§Ù„ Ø§Ø±ØªØ¨Ø§Ø·ÛŒ | Ù†ÛŒØ§Ø² Ø¨Ù‡ credential Ùˆ hardening provider | options validation + retry + attempts tracking |

## 13) Ø±ÛŒØ³Ú© Ù‡Ø§ÛŒ Ù…Ù‡Ù… Ø¨Ø±Ø§ÛŒ Ø³Ù‡Ø§Ù… Ø¯Ø§Ø±Ø§Ù†

### Ø±ÛŒØ³Ú© 1: Bootstrap admin Ø«Ø§Ø¨Øª Ø¯Ø± seeder
Ø¯Ø± `AuthorizationSeeder` Ø§ÛŒÙ…ÛŒÙ„/Ù¾Ø³ÙˆØ±Ø¯ bootstrap Ø¨Ù‡ ØµÙˆØ±Øª hard-coded ÙˆØ¬ÙˆØ¯ Ø¯Ø§Ø±Ø¯.
- Ø§Ø«Ø±: Ø±ÛŒØ³Ú© Ø§Ù…Ù†ÛŒØªÛŒ Ø¨Ø­Ø±Ø§Ù†ÛŒ Ø¯Ø± ØµÙˆØ±Øª Ø§Ø³ØªÙØ§Ø¯Ù‡ Ù†Ø§Ø®ÙˆØ§Ø³ØªÙ‡ Ø¯Ø± Ù…Ø­ÛŒØ· ÙˆØ§Ù‚Ø¹ÛŒ
- Ø§Ù‚Ø¯Ø§Ù… ÙÙˆØ±ÛŒ: Ø­Ø°Ù credential Ø«Ø§Ø¨ØªØŒ Ø§Ù†ØªÙ‚Ø§Ù„ Ø¨Ù‡ secret manager Ùˆ provision Ø§Ù…Ù†

### Ø±ÛŒØ³Ú© 2: Ú©Ù„ÛŒØ¯Ù‡Ø§ÛŒ Ù¾ÛŒØ´ ÙØ±Ø¶ Ø¯Ø± config Ù†Ù…ÙˆÙ†Ù‡
Ú©Ù„ÛŒØ¯Ù‡Ø§ÛŒÛŒ Ù…Ø§Ù†Ù†Ø¯ `Notifications:SensitiveDataEncryptionKey` Ø¯Ø± Ù†Ù…ÙˆÙ†Ù‡ config Ù†ÛŒØ§Ø² Ø¨Ù‡ override Ù‚Ø·Ø¹ÛŒ Ø¯Ø± production Ø¯Ø§Ø±Ù†Ø¯.
- Ø§Ø«Ø±: Ø±ÛŒØ³Ú© Ø§ÙØ´Ø§ÛŒ Ø¯Ø§Ø¯Ù‡/Ø¶Ø¹Ù Ø±Ù…Ø²Ù†Ú¯Ø§Ø±ÛŒ
- Ø§Ù‚Ø¯Ø§Ù…: enforce policy Ø¯Ø± startup Ùˆ CI

### Ø±ÛŒØ³Ú© 3: Ø±Ø´Ø¯ Ø¯Ø§Ù…Ù†Ù‡ Ø¯Ø± ÛŒÚ© deployable ÙˆØ§Ø­Ø¯
Ø¨Ø§ Ø§ÙØ²Ø§ÛŒØ´ ØªÛŒÙ…/Ø¯Ø§Ù…Ù†Ù‡ØŒ build/deploy time Ùˆ coupling Ù…Ù…Ú©Ù† Ø§Ø³Øª Ø±Ø´Ø¯ Ú©Ù†Ø¯.
- Ø§Ù‚Ø¯Ø§Ù…: Ø§Ø¯Ø§Ù…Ù‡ module boundary tests Ùˆ ØªØ¹Ø±ÛŒÙ migration plan Ø¨Ø±Ø§ÛŒ Ø³Ø±ÙˆÛŒØ³ Ø³Ø§Ø²ÛŒ Ø§Ù†ØªØ®Ø§Ø¨ÛŒ

## 14) Ø¬Ù…Ø¹ Ø¨Ù†Ø¯ÛŒ Ø§Ø¬Ø±Ø§ÛŒÛŒ
Ù…Ø¹Ù…Ø§Ø±ÛŒ ÙØ¹Ù„ÛŒ Ø¨Ø±Ø§ÛŒ ÙØ§Ø² Â«Ø±Ø´Ø¯ Ú©Ù†ØªØ±Ù„ Ø´Ø¯Ù‡ Ø¨Ø§ ØªÙ…Ø±Ú©Ø² Ø¨Ø± Ø§Ù…Ù†ÛŒØª Ùˆ Ù‚Ø§Ø¨Ù„ÛŒØª Ø§Ø·Ù…ÛŒÙ†Ø§Ù†Â» Ù…Ù†Ø§Ø³Ø¨ Ùˆ Ù‚Ø§Ø¨Ù„ Ø¯ÙØ§Ø¹ Ø§Ø³Øª.  
Ø¨Ø±Ø§ÛŒ ÙˆØ±ÙˆØ¯ Ø¨Ù‡ Ù…Ù‚ÛŒØ§Ø³ Ø¨Ø²Ø±Ú¯ØªØ±ØŒ Ø³Ù‡ Ø§Ù‚Ø¯Ø§Ù… Ø§ÙˆÙ„ÙˆÛŒØª Ø¨Ø§Ù„Ø§:
1. Ø³Ø®Øª Ú¯ÛŒØ±ÛŒ Ø§Ù…Ù†ÛŒØªÛŒ Ø¯Ø± secrets/bootstrap
2. ØªÙ‚ÙˆÛŒØª governance Ù…Ø±Ø² Ù…Ø§Ú˜ÙˆÙ„ Ù‡Ø§
3. ØªØ¹Ø±ÛŒÙ roadmap Ø³Ø±ÙˆÛŒØ³ Ø³Ø§Ø²ÛŒ Ø§Ù†ØªØ®Ø§Ø¨ÛŒ Ø¨Ø± Ø§Ø³Ø§Ø³ Ø¯Ø§Ø¯Ù‡ Ù‡Ø§ÛŒ ÙˆØ§Ù‚Ø¹ÛŒ Ø¨Ø§Ø± Ùˆ ØªÛŒÙ…


