param(
    [Parameter(Mandatory = $true)][string]$SqlServer,
    [Parameter(Mandatory = $true)][string]$Database,
    [Parameter(Mandatory = $true)][string]$User,
    [Parameter(Mandatory = $true)][string]$Password,
    [Parameter(Mandatory = $true)][string]$BackupPath
)

$singleUser = "ALTER DATABASE [$Database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;"
$restore = "RESTORE DATABASE [$Database] FROM DISK = N'$BackupPath' WITH REPLACE, CHECKSUM, STATS = 5;"
$multiUser = "ALTER DATABASE [$Database] SET MULTI_USER;"

sqlcmd -S $SqlServer -U $User -P $Password -Q $singleUser -C
sqlcmd -S $SqlServer -U $User -P $Password -Q $restore -C
sqlcmd -S $SqlServer -U $User -P $Password -Q $multiUser -C
