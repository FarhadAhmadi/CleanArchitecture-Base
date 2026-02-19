param(
    [Parameter(Mandatory = $true)]
    [string]$SmtpHost,

    [int]$Port = 587,

    [bool]$UseSsl = $true,

    [Parameter(Mandatory = $true)]
    [string]$UserName,

    [Parameter(Mandatory = $true)]
    [string]$Password,

    [Parameter(Mandatory = $true)]
    [string]$FromAddress,

    [string]$FromName = "NextGen Notifications",

    [bool]$IsBodyHtml = $true,

    [bool]$EnableNotifications = $true,

    [string]$SensitiveDataEncryptionKey = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectPath = "src/Web.Api/Web.Api.csproj"

if (-not (Test-Path $projectPath)) {
    throw "Web API project not found at '$projectPath'. Run this script from repository root."
}

function Set-Secret([string]$key, [string]$value) {
    dotnet user-secrets set $key $value --project $projectPath | Out-Null
}

Set-Secret "Notifications:Enabled" $EnableNotifications.ToString().ToLowerInvariant()
Set-Secret "Notifications:Email:Enabled" "true"
Set-Secret "Notifications:Email:Host" $SmtpHost
Set-Secret "Notifications:Email:Port" $Port.ToString()
Set-Secret "Notifications:Email:UseSsl" $UseSsl.ToString().ToLowerInvariant()
Set-Secret "Notifications:Email:UserName" $UserName
Set-Secret "Notifications:Email:Password" $Password
Set-Secret "Notifications:Email:FromAddress" $FromAddress
Set-Secret "Notifications:Email:FromName" $FromName
Set-Secret "Notifications:Email:IsBodyHtml" $IsBodyHtml.ToString().ToLowerInvariant()

if ([string]::IsNullOrWhiteSpace($SensitiveDataEncryptionKey)) {
    $SensitiveDataEncryptionKey = [Guid]::NewGuid().ToString("N")
}

Set-Secret "Notifications:SensitiveDataEncryptionKey" $SensitiveDataEncryptionKey

Write-Host ""
Write-Host "Email settings saved to user-secrets for '$projectPath'."
Write-Host "Run API:"
Write-Host "  dotnet run --project $projectPath"
Write-Host ""
Write-Host "To verify secret values:"
Write-Host "  dotnet user-secrets list --project $projectPath"
