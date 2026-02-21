# Clean Architecture Template

What's included in the template?

- SharedKernel project with common Domain-Driven Design abstractions.
- Domain layer with sample entities.
- Application layer with abstractions for:
  - CQRS
  - Example use cases
  - Cross-cutting concerns (logging, validation)
- Infrastructure layer with:
  - Authentication
  - Permission authorization
  - EF Core, SQL Server
  - Serilog
  - Outbox/Inbox reliability primitives
- Seq for searching and analyzing structured logs
  - Seq is available at http://localhost:8081 by default
- Observability stack:
  - Elasticsearch at http://localhost:9200
  - Kibana at http://localhost:5601
- Testing projects
  - Architecture testing

I'm open to hearing your feedback about the template and what you'd like to see in future iterations.

If you're ready to learn more, check out [**Pragmatic Clean Architecture**](https://www.milanjovanovic.tech/pragmatic-clean-architecture?utm_source=ca-template):

- Domain-Driven Design
- Role-based authorization
- Permission-based authorization
- Distributed caching with Redis
- OpenTelemetry
- Outbox pattern
- API Versioning
- Unit testing
- Functional testing
- Integration testing

Stay awesome!

## Quick Run (No Hassle)

From project root:

```powershell
.\scripts\dev\dev-stack.ps1 up
```

Windows shortcut:

```cmd
run-dev.cmd
```

Useful shortcuts:

```powershell
.\scripts\dev\dev-stack.ps1 status
.\scripts\dev\dev-stack.ps1 logs
.\scripts\dev\dev-stack.ps1 down
```

Detailed Persian quick guide:
`docs/operations/Quick-Run-Guide-fa.md`

Persian required configuration guide:
`docs/operations/Run-Required-Config-fa.md`

Persian architecture index:
`docs/architecture/README-fa.md`

Persian module docs index:
`docs/modules/README-fa.md`

## Operational baseline added

- Secret management via `SecretManagement` section, optional Azure Key Vault provider.
- JWT key rotation support via `Jwt:PreviousSecrets`.
- Controlled migration strategy via `DatabaseMigrations` options and CI artifact (`migrations.sql`).
- Outbox/Inbox infrastructure with:
  - background outbox processor
  - RabbitMQ publisher and inbox consumer worker
- Resilience policies:
  - SQL retry strategy (`EnableRetryOnFailure`)
  - Polly retry pipelines in background workers
  - HTTP standard resilience handler
- Redis distributed caching for authorization permissions with versioned invalidation.
- Security hardening:
  - security headers middleware
  - HSTS in production
  - per-user/per-IP rate limiting
  - CI security workflow (`.github/workflows/security.yml`)
- Audit trail (tamper-evident checksum chain):
  - `GET /api/v1/audit`
  - `GET /api/v1/audit/integrity`
- Operational metrics endpoint: `GET /api/v1/observability/metrics` (permission: `observability.read`).
- Operational alerting worker with webhook/PagerDuty support (`OperationalAlerting` section).
- Blue/green release strategy assets:
  - Kubernetes manifests under `deploy/k8s`
  - deployment switch script `deploy/k8s/blue-green-deploy.sh`
- Docker assets:
  - active build file: `src/Web.Api/Dockerfile`
  - versioned alternatives: `src/Web.Api/docker/Dockerfile.net8`, `src/Web.Api/docker/Dockerfile.net9`
- Infrastructure as code (Azure Terraform) under `infra/terraform` with `dev/staging/prod` tfvars.
- DR readiness:
  - backup/restore/drill scripts under `scripts/dr`
  - runbook at `docs/operations/backup-restore-runbook.md`
  - scheduled drill workflow `.github/workflows/dr-drill.yml`
- Performance test assets with k6 under `tests/Performance` and workflow `.github/workflows/performance.yml`.
