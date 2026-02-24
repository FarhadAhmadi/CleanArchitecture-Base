# Incident Runbook

## Severity Model
- `SEV-1`: Full outage, data loss risk, critical SLA breach
- `SEV-2`: Partial outage, major degradation, elevated 5xx
- `SEV-3`: Minor degradation, no immediate SLA breach

## Roles
- Incident Commander (IC): owns coordination
- Ops Lead: infrastructure and deployment actions
- App Lead: application diagnosis and fixes
- Comms Lead: stakeholder updates

## Immediate Actions (first 15 minutes)
1. Declare severity and assign IC.
2. Freeze non-essential deploys.
3. Capture baseline:
   - error rate, latency, affected endpoints
   - outbox/inbox status
   - last successful deploy and config changes
4. Start incident channel and timeline log.

## Diagnostic Flow
1. Check `GET /health` and `/api/v1/observability/metrics`.
2. Validate DB connectivity, pool saturation, lock waits.
3. Validate queue broker health and outbox processor lag.
4. Validate dependency health (cache, storage, SMTP, external APIs).
5. Correlate trace IDs for failing requests.
6. برای فایل‌ها:
   - `GET /ready` و check `file-storage`
   - نرخ `files.storage.object_not_found.count`
   - وضعیت‌های `Pending/Missing/Failed` در `files.FileAssets`

## Containment
- Roll back latest release if regression confirmed.
- Reduce traffic (rate limit hardening or temporary feature kill-switch).
- Disable non-critical background jobs if they amplify incident.
- فعال‌سازی auto-heal:
  - اجرای فوری reconciliation job برای همسان‌سازی DB و object storage
  - retry دستی فایل‌های `Pending` یا `Missing` بر اساس runbook فایل

## Recovery Criteria
- Error budget burn returns to normal.
- p95 latency under target.
- No data integrity gap in outbox/inbox.
- Monitoring stable for at least 30 minutes.

## Communication Cadence
- `SEV-1`: update every 15 minutes
- `SEV-2`: update every 30 minutes
- Include impact, ETA, workaround, next checkpoint.

## Post-Incident (within 48h)
1. Blameless postmortem.
2. Action items with owners and due dates.
3. Add prevention tests/alerts/runbook updates.
4. ثبت chaos scenario برای storage fault (timeout/object-not-found/circuit-open).
