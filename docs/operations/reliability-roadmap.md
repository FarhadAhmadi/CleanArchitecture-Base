# Reliability Roadmap

## Operational Definition of Done
- Availability: `>= 99.9%` for critical APIs.
- Read latency: `p95 < 300ms`.
- Error budget: `5xx < 0.1%`.
- Recovery: `RTO <= 30m`, `RPO <= 5m`.
- Scalability: sustain `3x` baseline load without SLA breach.

## Priority Backlog

### High
1. Atomic Outbox in one commit/transaction boundary.
2. Thin Web.Api (business orchestration moved to Application).
3. Idempotency and replay-safe processing for write paths and workers.
4. Operational observability with SLI dashboards and SLO alerts.

### Medium
1. File upload pipeline without full memory buffering.
2. Remove hardcoded admin checks and use permission/capability policy.
3. Distributed limits/locks for multi-instance deployments.
4. Tested DR/backup automation and periodic failover drills.

## Mandatory Quality Gates
- CI: build + architecture tests + contract tests + migration drift check.
- PR: no merge without green checks and regression coverage.
- Security: SAST + dependency scan + secret scan.
- Architecture fitness tests to prevent logic leakage into Web.Api.

## Delivery Phases
- Phase 1: correctness and guardrails (atomic outbox, idempotency, SLO baseline).
- Phase 2: architecture hardening (thin API layer, policy cleanup).
- Phase 3: scale and recovery (3x load, distributed controls, DR evidence).
