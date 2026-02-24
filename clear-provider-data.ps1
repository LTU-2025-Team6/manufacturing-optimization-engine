# Script to clear provider simulator database
# Deletes the Docker volume and restarts Gateway to regenerate data

Write-Host "Clearing provider database..." -ForegroundColor Yellow

# Stop all provider containers
Write-Host "Stopping provider containers..."
docker ps --filter "name=provider-" --format "{{.Names}}" | ForEach-Object {
    docker stop $_ | Out-Null
}

# Delete volume
Write-Host "Deleting volume..."
docker volume rm provider_simulator_data 2>$null

# Restart Gateway
Write-Host "Restarting Gateway..."
docker restart gateway | Out-Null

Write-Host "Done! Database will be recreated with fresh demo data." -ForegroundColor Green
