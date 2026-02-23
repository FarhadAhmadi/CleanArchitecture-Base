# Architecture Fitness Rules

## Layering Rules
1. `Domain` must not depend on `Application`, `Infrastructure`, `Web.Api`.
2. `Application` must not depend on `Infrastructure` or `Web.Api`.
3. `Web.Api` must stay thin:
   - endpoint mapping
   - auth/validation/filtering
   - no heavy orchestration logic

## Module Rules
1. Cross-module writes only through approved module ports.
2. Module contracts are versioned when payload shape changes.
3. ACL logic must be consistent between list and item-level access paths.

## Reliability Rules
1. Every critical state change that emits integration events must use atomic outbox.
2. Retryable operations must be idempotent.
3. Background workers must have bounded retries and poison/failure visibility.

## Operational Rules
1. Every critical API must emit trace correlation metadata.
2. SLO-backed metrics must be queryable in dashboard and alerting.
3. Runbooks must exist for incident and DR with drill evidence.

## CI Enforcement
- `ArchitectureTests` are required.
- Contract tests are required for API snapshot stability.
- Security workflow (SAST, dependency, secret scan) is required.
