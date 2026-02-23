# SLO / SLI Specification

## Scope
- Critical APIs:
  - `GET /health`
  - Read endpoints under `/api/v1/files/*`, `/api/v1/notifications*`, `/api/v1/todos*`
  - Auth endpoints: `/api/v1/users/login`, `/api/v1/users/refresh`

## Objectives (30-day rolling window)
- Availability: `>= 99.9%`
- Read latency: `p95 < 300ms`, `p99 < 700ms`
- Error budget: `5xx rate < 0.1%`
- Recovery: `RTO <= 30m`, `RPO <= 5m`

## SLI Definitions
- Availability SLI:
  - `successful_requests / total_requests`
  - Success: HTTP `2xx`, `3xx`, and expected `4xx` (except `429` for critical APIs)
- Latency SLI:
  - `http.server.duration` by route and method
  - Read APIs are measured separately from write APIs
- Error SLI:
  - `count(5xx) / count(total)`
- Outbox durability SLI:
  - `OutboxPending`
  - `OutboxFailed`
  - `OutboxOldestPendingAgeSeconds`

## Alert Policies
- Page (urgent):
  - Availability < `99.5%` for 10 minutes
  - `5xx > 1%` for 5 minutes
  - `OutboxFailed > 0` for 5 minutes
  - `OutboxOldestPendingAgeSeconds > 900`
- Ticket (non-urgent):
  - `p95 >= 300ms` for 30 minutes
  - `OutboxPending > 1000` for 15 minutes

## Dashboards (required panels)
- API:
  - Request rate, error rate, p95/p99 latency by endpoint
- Platform:
  - CPU, memory, GC pauses, thread pool saturation
- Orchestration:
  - Outbox pending/failed/age
  - Inbox pending/failed
  - Queue depths

## Error Budget Policy
- Monthly budget: `0.1%` 5xx
- Burn rate thresholds:
  - Fast burn: > 10x target over 1h => feature freeze + incident command
  - Slow burn: > 2x target over 24h => reliability sprint item mandatory
