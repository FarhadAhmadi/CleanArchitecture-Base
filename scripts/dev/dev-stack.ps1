param(
    [ValidateSet("up", "down", "restart", "status", "logs", "build", "test")]
    [string]$Action = "up",
    [switch]$SkipBuild,
    [switch]$SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Invoke-Step([string]$Title, [scriptblock]$Script) {
    Write-Host ""
    Write-Host "==> $Title" -ForegroundColor Cyan
    & $Script
}

function Wait-Container([string]$Name, [int]$TimeoutSec = 120) {
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        $status = docker inspect -f "{{.State.Status}}" $Name 2>$null
        if ($LASTEXITCODE -eq 0 -and $status -eq "running") {
            return
        }

        Start-Sleep -Seconds 2
    }

    throw "Container '$Name' did not reach running state in $TimeoutSec seconds."
}

function Wait-Healthy([string]$Name, [int]$TimeoutSec = 180) {
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        $health = docker inspect -f "{{if .State.Health}}{{.State.Health.Status}}{{else}}none{{end}}" $Name 2>$null
        if ($LASTEXITCODE -eq 0 -and ($health -eq "healthy" -or $health -eq "none")) {
            return
        }

        Start-Sleep -Seconds 3
    }

    throw "Container '$Name' did not become healthy in $TimeoutSec seconds."
}

function Ensure-Tool([string]$Tool, [string]$Hint) {
    if (-not (Get-Command $Tool -ErrorAction SilentlyContinue)) {
        throw "$Tool is not installed. $Hint"
    }
}

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $projectRoot

Ensure-Tool "docker" "Install Docker Desktop and enable Docker Compose."
Ensure-Tool "dotnet" "Install .NET SDK 10.x (or project target SDK)."

$services = @(
    "sqlserver",
    "rabbitmq",
    "redis",
    "minio",
    "seq",
    "elasticsearch",
    "kibana",
    "logstash",
    "web-api"
)

switch ($Action) {
    "up" {
        if (-not $SkipBuild) {
            Invoke-Step "Building solution" { dotnet build "CleanArchitecture.slnx" -c Debug }
        }

        Invoke-Step "Starting Docker stack" { docker compose up -d --build @services }
        Invoke-Step "Waiting for containers" {
            foreach ($service in $services) {
                Wait-Container -Name $service -TimeoutSec 180
            }
            Wait-Healthy -Name "sqlserver" -TimeoutSec 240
        }

        if (-not $SkipTests) {
            Invoke-Step "Running quick tests" { dotnet test "CleanArchitecture.slnx" -c Debug --no-build }
        }

        Invoke-Step "Stack status" { docker compose ps }

        Write-Host ""
        Write-Host "Ready URLs:"
        Write-Host "API:        http://localhost:5000/swagger"
        Write-Host "Health:     http://localhost:5000/health"
        Write-Host "Seq:        http://localhost:8081"
        Write-Host "Kibana:     http://localhost:5601"
        Write-Host "RabbitMQ:   http://localhost:15672"
        Write-Host "MinIO API:  http://localhost:9000"
        Write-Host "MinIO UI:   http://localhost:9001"
        break
    }
    "down" {
        Invoke-Step "Stopping Docker stack" { docker compose down }
        break
    }
    "restart" {
        Invoke-Step "Restarting Docker stack" { docker compose down; docker compose up -d --build @services }
        Invoke-Step "Stack status" { docker compose ps }
        break
    }
    "status" {
        Invoke-Step "Stack status" { docker compose ps }
        break
    }
    "logs" {
        Invoke-Step "Live logs (Ctrl+C to exit)" { docker compose logs -f --tail=200 @services }
        break
    }
    "build" {
        Invoke-Step "Building solution" { dotnet build "CleanArchitecture.slnx" -c Debug }
        break
    }
    "test" {
        Invoke-Step "Running tests" { dotnet test "CleanArchitecture.slnx" -c Debug --no-build }
        break
    }
    default {
        throw "Unsupported action: $Action"
    }
}
