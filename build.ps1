#!/usr/bin/env pwsh
param(
    [string]$Configuration = "Release",
    [string]$Platform = "Any CPU"
)

Write-Host "Building Fiddler to Swagger YAML Exporter..." -ForegroundColor Green
Write-Host ""

Push-Location workspace

try {
    Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
    $restoreResult = & nuget restore FiddlerToSwagger.sln 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to restore NuGet packages" -ForegroundColor Red
        Write-Host "Please ensure NuGet is installed and available in PATH" -ForegroundColor Red
        Write-Host $restoreResult -ForegroundColor Red
        exit 1
    }

    Write-Host ""
    Write-Host "Building solution..." -ForegroundColor Yellow
    $buildResult = & msbuild FiddlerToSwagger.sln /p:Configuration=$Configuration /p:Platform=$Platform /verbosity:minimal 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Build failed" -ForegroundColor Red
        Write-Host $buildResult -ForegroundColor Red
        exit 1
    }

    Write-Host ""
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "The extension has been automatically copied to your Fiddler ImportExport folder." -ForegroundColor Cyan
    Write-Host "Restart Fiddler to use the new extension." -ForegroundColor Cyan
    Write-Host ""
}
catch {
    Write-Host "ERROR: An unexpected error occurred" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
} 