# --- Configuration ---
$InfraPath = $PSScriptRoot
$EnvPath = Join-Path $InfraPath ".env"
$BACKUP_DIR = "C:\backup"
$DATE = Get-Date -Format "yyyy-MM-dd_HHmm"
$FILENAME = "IAM_backup_$DATE.dump"
$FULL_PATH = Join-Path $BACKUP_DIR $FILENAME

if (Test-Path $EnvPath) {
    Get-Content $EnvPath | Where-Object { $_ -match "=" } | ForEach-Object {
        $name, $value = $_.Split('=', 2)
        Set-Variable -Name "ENV_$name" -Value $value.Trim() -Scope Script
    }
}

$CONTAINER_NAME = if ($ENV_POSTGRES_SERVICE_NAME) { $ENV_POSTGRES_SERVICE_NAME } else { "postgres_db" }
$DATABASE_NAME = if ($ENV_POSTGRES_DB) { $ENV_POSTGRES_DB } else { "iam" }
$DATABASE_USER = if ($ENV_POSTGRES_USER) { $ENV_POSTGRES_USER } else { "admin" }
$CONTAINER_BACKUP_PATH = "/tmp/$FILENAME"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "PostgreSQL Docker Backup Routine"
Write-Host "==========================================" -ForegroundColor Cyan

if (-not (Test-Path $BACKUP_DIR)) {
    New-Item -ItemType Directory -Path $BACKUP_DIR | Out-Null
    Write-Host "[INFO] Created directory $BACKUP_DIR" -ForegroundColor Gray
}

$containerStatus = docker inspect -f '{{.State.Running}}' $CONTAINER_NAME 2>$null
if ($containerStatus -ne "true") {
    Write-Host "[ERROR] Container $CONTAINER_NAME is not running. Cannot backup." -ForegroundColor Red
    exit 1
}

Write-Host "[INFO] Starting backup of database: $DATABASE_NAME..." -ForegroundColor Cyan

docker exec $CONTAINER_NAME pg_dump -U $DATABASE_USER -Fc -f $CONTAINER_BACKUP_PATH $DATABASE_NAME
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Backup failed during pg_dump." -ForegroundColor Red
    exit 1
}

docker cp "${CONTAINER_NAME}:$CONTAINER_BACKUP_PATH" $FULL_PATH
$copyExitCode = $LASTEXITCODE
docker exec $CONTAINER_NAME rm $CONTAINER_BACKUP_PATH | Out-Null

if ($copyExitCode -eq 0) {
    $size = (Get-Item $FULL_PATH).Length / 1MB
    Write-Host "[SUCCESS] Backup saved to: $FULL_PATH" -ForegroundColor Green
    Write-Host "[INFO] Backup size: $([math]::Round($size, 2)) MB" -ForegroundColor Gray
} else {
    Write-Host "[ERROR] Backup copy failed." -ForegroundColor Red
    exit 1
}

Write-Host "[INFO] Cleaning up backups older than 7 days..." -ForegroundColor Gray
Get-ChildItem $BACKUP_DIR -Filter "IAM_backup_*.dump" |
    Where-Object { $_.CreationTime -lt (Get-Date).AddDays(-7) } |
    Remove-Item

Write-Host "Done." -ForegroundColor Cyan

####################################################################
########################## How to Restore ##########################
# docker cp C:\backup\IAM_backup_yyyy-MM-dd_HHmm.dump postgres_db:/tmp/IAM_backup.dump
# docker exec -i postgres_db pg_restore -U admin -d iam -c --clean /tmp/IAM_backup.dump
