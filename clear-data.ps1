# Script to clear all databases
# Deletes Docker volumes and local database files

Write-Host "Clearing all databases..." -ForegroundColor Yellow

# Stop and remove all containers
Write-Host "Stopping all containers..."
docker-compose down

# Force remove specific containers if they still exist
Write-Host "Removing containers..."
docker rm -f rabbitmq gateway engine 2>$null | Out-Null

# Stop and remove all provider containers if any are running standalone
docker ps -a --filter "name=provider-" --format "{{.Names}}" | ForEach-Object {
    docker rm -f $_ 2>$null | Out-Null
}

# Delete local database files from bin folders
Write-Host "Deleting local database files..."
Get-ChildItem -Path . -Filter "*.db*" -Recurse -ErrorAction SilentlyContinue | Where-Object { 
    $_.FullName -match "\\bin\\" -and ($_.Name -match "^(gateway)\.db") 
} | ForEach-Object {
    Write-Host "  Removing file: $($_.FullName)"
    Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
}

# Delete all Docker database volumes - find by name pattern
Write-Host "Deleting Docker volumes..."
docker volume ls --format "{{.Name}}" | Where-Object { 
    $_ -match "gateway_data$" -or 
    $_ -match "rabbitmq_data$" -or
    $_ -eq "provider_simulator_data"
} | ForEach-Object {
    Write-Host "  Removing volume: $_"
    docker volume rm $_ 2>$null | Out-Null
}

# Wait for cleanup to complete
Start-Sleep -Seconds 2

Write-Host "Done! All databases cleared." -ForegroundColor Green
Write-Host "You can now start the services from Visual Studio or run 'docker-compose up -d'" -ForegroundColor Cyan
