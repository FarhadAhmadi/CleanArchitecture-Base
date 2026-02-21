# Backup / Restore / DR Runbook

## Preconditions
- `sqlcmd` is installed on the operator machine.
- SQL account has backup/restore permissions.
- Backup storage path is writable by SQL Server.

## Backup
```powershell
pwsh scripts/dr/backup.ps1 `
  -SqlServer "tcp:prod-sql.database.windows.net,1433" `
  -Database "CleanArchitectureBase" `
  -User "sqladminuser" `
  -Password "<secret>" `
  -BackupPath "D:\backups\cleanarch.bak"
```

## Restore
```powershell
pwsh scripts/dr/restore.ps1 `
  -SqlServer "tcp:prod-sql.database.windows.net,1433" `
  -Database "CleanArchitectureBase-Restore" `
  -User "sqladminuser" `
  -Password "<secret>" `
  -BackupPath "D:\backups\cleanarch.bak"
```

## DR Drill
```powershell
pwsh scripts/dr/drill.ps1 `
  -SqlServer "tcp:prod-sql.database.windows.net,1433" `
  -SourceDatabase "CleanArchitectureBase" `
  -DrillDatabase "CleanArchitectureBase-Drill" `
  -User "sqladminuser" `
  -Password "<secret>" `
  -BackupPath "D:\backups\cleanarch-drill.bak"
```

## RPO / RTO Checklist
- Capture start/end times for every drill.
- Record measured `RPO` and `RTO`.
- If `RTO` or `RPO` breaches policy, open incident and action plan.

## Post-Restore Verification (Required)
1. Run API health check: `GET /health`
2. Verify critical module tables are accessible:
```sql
SELECT TOP 1 Id FROM users.Users;
SELECT TOP 1 Id FROM notifications.NotificationMessages;
SELECT TOP 1 Id FROM scheduler.ScheduledJobs;
```
3. Validate scheduler pipeline quickly:
   - create a one-time job
   - confirm at least one record in `scheduler.JobExecutions`
