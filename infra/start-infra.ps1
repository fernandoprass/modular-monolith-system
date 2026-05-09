
#Requires -Version 5.1
<#
.SYNOPSIS
    Starts the full local infrastructure: PostgreSQL, MongoDB, and Redis.

.DESCRIPTION
    1. Ensures the shared Docker network exists.
    2. Brings up each compose stack (postgres → mongodb → redis).

.PARAMETER Down
    Tear down all stacks instead of starting them.

.PARAMETER Seed
    Run the DB seed script (iam_db-setup.ps1) after PostgreSQL is healthy.

.PARAMETER Pull
    Pull the latest images before starting.

.EXAMPLE
    .\start-infra.ps1              # Start everything
    .\start-infra.ps1 -Seed        # Start + seed the Postgres DB
    .\start-infra.ps1 -Down        # Stop and remove all containers
    .\start-infra.ps1 -Pull        # Pull fresh images, then start
#>

[CmdletBinding()]
param(
    [switch]$Down,
    [switch]$Seed,
    [switch]$Pull
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
function Write-Step([string]$Message) {
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

function Write-Success([string]$Message) {
    Write-Host "    [OK] $Message" -ForegroundColor Green
}

function Write-Fail([string]$Message) {
    Write-Host "    [FAIL] $Message" -ForegroundColor Red
}

function Assert-DockerRunning {
    Write-Step "Checking Docker daemon..."
    try {
        docker info --format '{{.ServerVersion}}' | Out-Null
        Write-Success "Docker is running."
    }
    catch {
        Write-Fail "Docker is not running or not installed. Please start Docker Desktop."
        exit 1
    }
}

function Ensure-Network([string]$NetworkName) {
    Write-Step "Ensuring Docker network '$NetworkName' exists..."
    $existing = docker network ls --filter "name=^${NetworkName}$" --format '{{.Name}}'
    if ($existing -eq $NetworkName) {
        Write-Success "Network '$NetworkName' already exists."
    }
    else {
        docker network create $NetworkName | Out-Null
        Write-Success "Network '$NetworkName' created."
    }
}

function Get-EnvValue([string]$Key, [string]$EnvFile = ".env") {
    if (-not (Test-Path $EnvFile)) { return $null }
    $line = Get-Content $EnvFile | Where-Object { $_ -match "^\s*${Key}\s*=" } | Select-Object -First 1
    if ($line) { return ($line -split '=', 2)[1].Trim() }
    return $null
}

# ---------------------------------------------------------------------------
# Config
# ---------------------------------------------------------------------------
$ScriptDir   = $PSScriptRoot
$EnvFile     = Join-Path $ScriptDir ".env"
$NetworkName = Get-EnvValue "NETWORK_NAME" $EnvFile
if (-not $NetworkName) { $NetworkName = "modular-network" }

$Stacks = @(
    @{ Name = "PostgreSQL"; File = "docker-compose.postgresql.yaml" },
    @{ Name = "MongoDB";    File = "docker-compose.mongodb.yaml"  },
    @{ Name = "Redis";      File = "docker-compose.redis.yaml"    }
)

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
Assert-DockerRunning

if ($Down) {
    Write-Step "Tearing down all stacks..."
    foreach ($stack in [Linq.Enumerable]::Reverse($Stacks)) {
        $file = Join-Path $ScriptDir $stack.File
        if (Test-Path $file) {
            Write-Host "  Stopping $($stack.Name)..." -ForegroundColor Yellow
            docker compose --env-file $EnvFile -f $file down
        }
    }
    Write-Success "All stacks stopped."
    exit 0
}

Ensure-Network $NetworkName

# Optionally pull latest images first
if ($Pull) {
    Write-Step "Pulling latest images..."
    foreach ($stack in $Stacks) {
        $file = Join-Path $ScriptDir $stack.File
        if (Test-Path $file) {
            Write-Host "  Pulling $($stack.Name)..." -ForegroundColor Yellow
            docker compose --env-file $EnvFile -f $file pull
        }
    }
}

# Start each stack
foreach ($stack in $Stacks) {
    $file = Join-Path $ScriptDir $stack.File

    if (-not (Test-Path $file)) {
        Write-Fail "$($stack.Name) compose file not found: $file — skipping."
        continue
    }

    Write-Step "Starting $($stack.Name) ($($stack.File))..."
    docker compose --env-file $EnvFile -f $file up -d --remove-orphans

    if ($LASTEXITCODE -ne 0) {
        Write-Fail "$($stack.Name) failed to start. Check the logs:"
        Write-Host "  docker compose -f $($stack.File) logs --tail=50" -ForegroundColor Yellow
        exit $LASTEXITCODE
    }

    Write-Success "$($stack.Name) is up."
}

# ---------------------------------------------------------------------------
# Optional DB seed
# ---------------------------------------------------------------------------
#if ($Seed) {
#    $seedScript = Join-Path $ScriptDir "iam_seed.ps1"
#    if (Test-Path $seedScript) {
#        Write-Step "Running DB seed script: iam_seed.ps1"
#        & $seedScript
#        Write-Success "Seed complete."
#    }
#    else {
#        Write-Host "    [WARN] -Seed specified but iam_seed.ps1 not found — skipping." -ForegroundColor Yellow
#    }
#}

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "=============================================" -ForegroundColor Magenta
Write-Host "  Infrastructure is UP" -ForegroundColor Magenta
Write-Host "=============================================" -ForegroundColor Magenta
Write-Host "  pgAdmin      ->  http://localhost:$(Get-EnvValue 'PGADMIN_PORT' $EnvFile)"
Write-Host "  Mongo Express->  http://localhost:$(Get-EnvValue 'MONGO_EXPRESS_PORT' $EnvFile)"
Write-Host "  PostgreSQL   ->  localhost:$(Get-EnvValue 'POSTGRES_PORT' $EnvFile)"
Write-Host "  MongoDB      ->  localhost:$(Get-EnvValue 'MONGO_PORT' $EnvFile)"
Write-Host "  Redis        ->  localhost:$(Get-EnvValue 'REDIS_PORT' $EnvFile)"
Write-Host "=============================================" -ForegroundColor Magenta

000000000000000000000000000

# 1. Load Environment Variables from .env
if (Test-Path .env) {
    Get-Content .env | Where-Object { $_ -match '=' -and $_ -notmatch '^#' } | ForEach-Object {
        $name, $value = $_.Split('=', 2)
        Set-Variable -Name "ENV_$($name.Trim())" -Value $value.Trim() -Scope Script
    }
} else {
    Write-Error "Error: .env file not found!"
    exit
}

$netName = $script:ENV_NETWORK_NAME

# 2. Check if Docker Network exists
Write-Host "Checking network: $netName..." -ForegroundColor Cyan
$networkCheck = docker network ls --filter "name=^$($netName)$" --format "{{.Name}}"

if (-not $networkCheck) {
    Write-Host "Network not found. Creating network: $netName..." -ForegroundColor Yellow
    docker network create $netName
} else {
    Write-Host "Network '$netName' already exists." -ForegroundColor Green
}

# 3. Spin up Infrastructure Services
Write-Host "Starting infrastructure..." -ForegroundColor Cyan

Write-Host "-> Launching Redis"
docker-compose -f docker-compose.redis.yaml up -d

Write-Host "-> Launching MongoDB & UI"
docker-compose -f docker-compose.mongodb.yaml up -d

Write-Host "Infrastructure is ready!" -ForegroundColor Green