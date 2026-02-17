param(
    [Parameter(Mandatory = $true)][string]$SqlServer,
    [Parameter(Mandatory = $true)][string]$Database,
    [Parameter(Mandatory = $true)][string]$User,
    [Parameter(Mandatory = $true)][string]$Password,
    [Parameter(Mandatory = $true)][string]$BackupPath
)

$query = "BACKUP DATABASE [$Database] TO DISK = N'$BackupPath' WITH INIT, COMPRESSION, CHECKSUM, STATS = 5;"
sqlcmd -S $SqlServer -U $User -P $Password -Q $query -C
