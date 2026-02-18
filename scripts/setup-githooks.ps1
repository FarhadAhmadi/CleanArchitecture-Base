Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $projectRoot

git config core.hooksPath ".githooks"
Write-Host "Git hooks path configured to .githooks"
