# Removes all migrations and creates a fresh Initial migration for Gateway, Engine, and ProviderRegistry
# Also deletes all SQLite .db files in bin/Debug/net9.0 for each project
# Run this script from the solution root
$ErrorActionPreference = 'Stop'

# Configuration
$Projects = @(
    @{ Name = "ManufacturingOptimization.Gateway"; Csproj = "ManufacturingOptimization.Gateway.csproj" }
    @{ Name = "ManufacturingOptimization.Engine"; Csproj = "ManufacturingOptimization.Engine.csproj" }
    @{ Name = "ManufacturingOptimization.ProviderSimulator"; Csproj = "ManufacturingOptimization.ProviderSimulator.csproj" }
)

# Helper function to remove all .db files in the specified project's bin folder
function Remove-DbFiles {
    param([string]$ProjectFolder)
    
    $binPaths = @(
        "$ProjectFolder\bin\Debug\net9.0",
        "$ProjectFolder\bin\Debug\net9.0\Data"
    )
    
    foreach ($path in $binPaths) {
        if (Test-Path $path) {
            $dbFiles = Get-ChildItem -Path $path -Filter *.db -ErrorAction SilentlyContinue
            if ($dbFiles) {
                $dbFiles | Remove-Item -Force
            }
        }
    }
}

# Helper function to reset migrations for a project
function Reset-Migrations {
    param(
        [string]$ProjectFolder,
        [string]$CsprojName
    )
    
    # Remove existing database files
    Remove-DbFiles -ProjectFolder $ProjectFolder
    
    # Navigate to project folder
    Push-Location $ProjectFolder
    
    try {
        # Remove existing database
        dotnet ef database drop --project $CsprojName --startup-project $CsprojName --force 2>$null
        
        # Remove existing migrations
        if (Test-Path .\Migrations) { 
            Remove-Item -Recurse -Force .\Migrations 
        }
        
        # Build project
        dotnet build $CsprojName
        
        # Create new Initial migration
        dotnet ef migrations add Initial --project $CsprojName --startup-project $CsprojName
        
        # Apply migration to database
        dotnet ef database update --project $CsprojName --startup-project $CsprojName
        
        Write-Host "$ProjectFolder completed successfully"
    }
    catch {
        Write-Host "Error in $ProjectFolder : $_"
        throw
    }
    finally {
        Pop-Location
    }
}

# Main execution
Write-Host "Starting migration reset for all projects..."

foreach ($project in $Projects) {
    Reset-Migrations -ProjectFolder $project.Name -CsprojName $project.Csproj
}

Write-Host "All migrations reset and databases updated!"