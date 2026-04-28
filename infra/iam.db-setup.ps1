# --- Configuration ---
$ENV_FILE = ".env"
$DOCKER_COMPOSE_FILE = "iam.docker-compose.yml"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Optimized Setup"
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Hardware Auto-Detection
Write-Host "[INFO] Detecting System Resources..." -ForegroundColor Gray
$SystemRAM_GB = [Math]::Floor((Get-CimInstance Win32_PhysicalMemory | Measure-Object Capacity -Sum).Sum / 1GB)
if ($SystemRAM_GB -lt 1) { $SystemRAM_GB = 1 } # minimum 1GB
Write-Host "Detected: $SystemRAM_GB GB RAM" -ForegroundColor Green

# 2. Check/Create .env with dynamic RAM value
if (-not (Test-Path $ENV_FILE)) {
    Write-Host "[INFO] Creating .env with detected resources..." -ForegroundColor Yellow
    
    $defaultEnvContent = @"
# --- Database Identity ---
POSTGRES_USER=admin
POSTGRES_PASSWORD=msadmin123
POSTGRES_DB=IAM

# --- pgAdmin ---
PGADMIN_EMAIL=admin@admin.com
PGADMIN_PASSWORD=msadmin123

# --- Tuning Params (Passed to Docker Compose) ---
TOTAL_RAM_GB=$SystemRAM_GB
"@
    $defaultEnvContent | Set-Content -Path $ENV_FILE -Encoding UTF8
}

# 3. Load Environment Variables into PowerShell context
Get-Content $ENV_FILE | Where-Object { $_ -match '=' -and $_ -notmatch '^#' } | ForEach-Object {
    $name, $value = $_.Split('=', 2)
    [System.Environment]::SetEnvironmentVariable($name.Trim(), $value.Trim())
}

# 4. Docker Pre-flight Check
if (-not (Get-Process "Docker Desktop" -ErrorAction SilentlyContinue)) {
    Write-Host "[WARNING] Docker Desktop process not found. Checking engine status..." -ForegroundColor Yellow
}

& docker info >$null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Docker is not responding. Please start Docker Desktop." -ForegroundColor Red
    exit
}

# 5. Start and Monitor
Write-Host "[INFO] Cleaning up old containers..." -ForegroundColor Gray
docker-compose -f $DOCKER_COMPOSE_FILE down

Write-Host "[INFO] Starting containers..." -ForegroundColor Cyan
docker-compose -f $DOCKER_COMPOSE_FILE up -d --remove-orphans

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n[WAIT] Waiting for Database to be Healthy..." -ForegroundColor Yellow
    $counter = 0
    do {
        $status = docker inspect --format='{{.State.Health.Status}}' postgres_db 2>$null
        Write-Host "." -NoNewline
        Start-Sleep -Seconds 2
        $counter++
    } until ($status -eq "healthy" -or $counter -gt 20)

    if ($status -ne "healthy") {
        Write-Host "`n[WARNING] Database is taking too long to respond. Check 'docker logs postgres_db'." -ForegroundColor Yellow
    } else {
        Write-Host "`n----------------------------------------------------------" -ForegroundColor Green
        Write-Host "  Database environment is TUNED and READY!" -ForegroundColor Green
        Write-Host "  User:          $($env:POSTGRES_USER)"
        Write-Host "  RAM Allocated: $($env:TOTAL_RAM_GB) GB"
        Write-Host "  pgAdmin:       http://localhost:8080  or http://127.0.0.1:8080"
        Write-Host "----------------------------------------------------------"
    }
}