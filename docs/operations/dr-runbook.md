# DR Runbook

## Targets
- `RTO <= 30m`
- `RPO <= 5m`

## Preconditions
- Valid backup policy enabled (full + log backups / PITR)
- Restore access validated for DR operators
- Recent restore test passed

## DR Activation Checklist
1. Declare DR event and assign DR Commander.
2. Confirm primary failure mode (DB, region, platform).
3. Lock writes if split-brain risk exists.
4. Start restore/failover workflow.

## Restore Procedure
1. Select restore point by RPO target.
2. Restore DB to DR environment.
3. Apply pending migrations if required.
4. Boot API with DR connection string.
5. Validate smoke checks:
   - `/health`
   - auth/login
   - read/write on critical modules

## Verification Checklist
- Data consistency checks:
  - user count variance acceptable
  - outbox/inbox replay consistency
  - scheduler state sanity
- Functional checks:
  - file read path
  - notification queue processing
  - token issuance/refresh

## Failback Plan
1. Stabilize primary.
2. Replicate delta from DR back to primary.
3. Controlled cutover with readonly window.
4. Final verification + DR closeout.

## Drill Cadence
- Monthly tabletop
- Quarterly full restore drill
- Annual region failover simulation

## Evidence and Reporting
- Capture timestamps for each milestone.
- Record achieved RTO/RPO.
- File action items if targets are missed.
