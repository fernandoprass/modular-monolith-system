#Requires -Version 5.1
[CmdletBinding()]
param([switch]$Down, [switch]$Seed, [switch]$Pull)

$ErrorActionPreference = 'Continue'

# Mapeamento explícito para evitar erros de leitura no YAML
$Stacks = @(
    @{ File = "docker-compose.postgresql.yaml"; Container = "POSTGRES_SERVICE_NAME" },
    @{ File = "docker-compose.mongodb.yaml";    Container = "MONGO_SERVICE_NAME" },
    @{ File = "docker-compose.redis.yaml";      Container = "REDIS_SERVICE_NAME" }
)

function Get-Env($key) {
    if (-not (Test-Path .env)) { return "N/A" }
    $match = Select-String -Path .env -Pattern "^\s*$key\s*=(.*)" -ErrorAction SilentlyContinue
    if ($match) { return $match.Matches[0].Groups[1].Value.Trim() }
    return "N/A"
}

Write-Host "`n==> Checking Docker..." -ForegroundColor Cyan
docker version --format '{{.Server.Version}}' >$null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "    [ERROR] Docker is not running." -ForegroundColor Red
    exit 1
}

if ($Down) {
    Write-Host "==> Tearing down infrastructure..." -ForegroundColor Cyan
    foreach ($s in $Stacks) {
        if (Test-Path $s.File) { docker compose -f $s.File down -v --rmi all }
    }
    exit 0
}

# Garante que a rede exista ANTES de qualquer comando 'up'
$NetName = Get-Env "NETWORK_NAME"
if ($NetName -eq "N/A") { $NetName = "cms-network" }
if (-not (docker network ls --filter name=^$NetName$ -q)) {
    Write-Host "==> Creating network: $NetName" -ForegroundColor Cyan
    docker network create $NetName >$null
}

foreach ($s in $Stacks) {
    if (-not (Test-Path $s.File)) { continue }
    
    $containerName = Get-Env $s.Container
    
    # Verifica se já está saudável
    $status = docker inspect --format='{{.State.Health.Status}}' $containerName 2>$null
    if ($status -eq "healthy") {
        Write-Host "    [SKIP] $containerName is already healthy." -ForegroundColor Gray
        continue
    }

    Write-Host "    Launching $($s.File)..." -ForegroundColor Green
    if ($Pull) { docker compose -f $s.File pull }
    
    # IMPORTANT: --remove-orphans pode remover outros containers se os YAMLs 
    # não estiverem perfeitamente alinhados na definição da rede.
    docker compose -f $s.File up -d
    
    Write-Host "    Waiting for $containerName" -NoNewline -ForegroundColor Yellow
    $counter = 0
    do {
        Write-Host "." -NoNewline
        Start-Sleep -Seconds 2
        $status = docker inspect --format='{{.State.Health.Status}}' $containerName 2>$null
        $counter++
    } until ($status -eq "healthy" -or $counter -gt 15)
    Write-Host " [OK]" -ForegroundColor Green
}

if ($Seed -and (Test-Path "iam_db-setup.ps1")) {
    Write-Host "==> Seeding database..." -ForegroundColor Cyan
    .\iam_db-setup.ps1
}

# --- Final Summary ---
Write-Host "`n=============================================" -ForegroundColor Magenta
Write-Host "  INFRASTRUCTURE IS READY"                     -ForegroundColor Magenta
Write-Host "=============================================" -ForegroundColor Magenta
Write-Host "  pgAdmin       -> http://localhost:$(Get-Env 'PGADMIN_PORT')"
Write-Host "  Mongo Express -> http://localhost:$(Get-Env 'MONGO_EXPRESS_PORT')"
Write-Host "  PostgreSQL    -> localhost:$(Get-Env 'POSTGRES_PORT')"
Write-Host "  MongoDB       -> localhost:$(Get-Env 'MONGO_PORT')"
Write-Host "  Redis         -> localhost:$(Get-Env 'REDIS_PORT')"
Write-Host "=============================================`n" -ForegroundColor Magenta