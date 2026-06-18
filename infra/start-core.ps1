$InfraPath = $PSScriptRoot

docker compose --env-file (Join-Path $InfraPath ".env") -f (Join-Path $InfraPath "docker-compose.core.yaml") up --build core-api
