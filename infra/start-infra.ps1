#Requires -Version 5.1
[CmdletBinding()]
param([switch]$Down, [switch]$Seed, [switch]$Pull)

$ErrorActionPreference = 'Continue'
# Ensure these filenames match your actual files EXACTLY
$Files = @("docker-compose.postgresql.yaml", "docker-compose.mongodb.yaml", "docker-compose.redis.yaml")

function Get-Env($key) {
    if (-not (Test-Path .env)) { return "N/A" }
    $match = Select-String -Path .env -Pattern "^\s*$key\s*=(.*)" -ErrorAction SilentlyContinue
    if ($match) { return $match.Matches[0].Groups[1].Value.Trim() }
    return "N/A"
}

Write-Host "`n==> checking Docker daemon..." -ForegroundColor Cyan
docker version --format '{{.Server.Version}}' >$null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "    [ERROR] Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

if ($Down) {
    Write-Host "==> Tearing down everything..." -ForegroundColor Cyan
    foreach ($f in $Files) {
        if (Test-Path $f) { 
            Write-Host "    Stopping $f..." -ForegroundColor Yellow
            docker compose -f $f down -v --rmi all 
        }
    }
    exit 0
}

# IMPORTANT: The network must exist before starting the containers
Write-Host "==> Preparing Network..." -ForegroundColor Cyan
$NetName = Get-Env "NETWORK_NAME"
if ($NetName -eq "N/A") { $NetName = "cms-network" }
# Check if network exists, if not, create it
if (-not (docker network ls --filter name=^$NetName$ -q)) {
    docker network create $NetName
    Write-Host "    Network '$NetName' created." -ForegroundColor Green
}

Write-Host "==> Starting Services..." -ForegroundColor Cyan
foreach ($f in $Files) {
    if (Test-Path $f) {
        if ($Pull) { docker compose -f $f pull }
        Write-Host "    Launching $f..." -ForegroundColor Green
        # The 'up -d' command is what "starts the images" for you
        docker compose -f $f up -d --remove-orphans
    } else {
        Write-Host "    [ERROR] File not found: $f" -ForegroundColor Red
    }
}

if ($Seed -and (Test-Path "iam_db-setup.ps1")) {
    Write-Host "==> Seeding database..." -ForegroundColor Cyan
    .\iam_db-setup.ps1
}

# Final Summary
Write-Host "`n=============================================" -ForegroundColor Magenta
Write-Host "  Infrastructure is UP"                        -ForegroundColor Magenta
Write-Host "=============================================" -ForegroundColor Magenta
Write-Host "  pgAdmin       -> http://localhost:$(Get-Env 'PGADMIN_PORT')"
Write-Host "  Mongo Express -> http://localhost:$(Get-Env 'MONGO_EXPRESS_PORT')"
Write-Host "  PostgreSQL    -> localhost:$(Get-Env 'POSTGRES_PORT')"
Write-Host "  MongoDB       -> localhost:$(Get-Env 'MONGO_PORT')"
Write-Host "  Redis         -> localhost:$(Get-Env 'REDIS_PORT')"
Write-Host "=============================================`n" -ForegroundColor Magenta