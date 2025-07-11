# SystemInstaller Docker Startup Script
Write-Host "Starting SystemInstaller with Docker Compose..." -ForegroundColor Green

# Change to project directory
Set-Location "d:\proj\SystemInstaller"

# Start Docker containers
Write-Host "Starting containers..." -ForegroundColor Yellow
docker-compose up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nServices started successfully!" -ForegroundColor Green
    Write-Host "- Database: SQL Server on port 1433" -ForegroundColor Cyan
    Write-Host "- Keycloak: http://localhost:8080 (admin/admin123)" -ForegroundColor Cyan
    Write-Host "- Web App: http://localhost:8081" -ForegroundColor Cyan
    Write-Host "`nTo view logs: docker-compose logs -f" -ForegroundColor Yellow
    Write-Host "To stop: docker-compose down" -ForegroundColor Yellow
} else {
    Write-Host "Error starting containers. Check Docker is running." -ForegroundColor Red
}

Write-Host "`nPress any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
