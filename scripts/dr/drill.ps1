param(
    [Parameter(Mandatory = $true)][string]$SqlServer,
    [Parameter(Mandatory = $true)][string]$SourceDatabase,
    [Parameter(Mandatory = $true)][string]$DrillDatabase,
    [Parameter(Mandatory = $true)][string]$User,
    [Parameter(Mandatory = $true)][string]$Password,
    [Parameter(Mandatory = $true)][string]$BackupPath
)

Write-Host "Starting DR drill..."

& "$PSScriptRoot/backup.ps1" `
    -SqlServer $SqlServer `
    -Database $SourceDatabase `
    -User $User `
    -Password $Password `
    -BackupPath $BackupPath

$createDrillDb = "IF DB_ID('$DrillDatabase') IS NULL CREATE DATABASE [$DrillDatabase];"
sqlcmd -S $SqlServer -U $User -P $Password -Q $createDrillDb -C

& "$PSScriptRoot/restore.ps1" `
    -SqlServer $SqlServer `
    -Database $DrillDatabase `
    -User $User `
    -Password $Password `
    -BackupPath $BackupPath

$check = "SELECT COUNT(1) AS TotalUsers FROM [$DrillDatabase].[dbo].[Users];"
sqlcmd -S $SqlServer -U $User -P $Password -Q $check -C

Write-Host "DR drill completed successfully."
