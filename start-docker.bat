@echo off
echo Starting SystemInstaller with Docker Compose...
cd /d "d:\proj\SystemInstaller"
docker-compose up -d
echo.
echo Services starting...
echo - Database: SQL Server on port 1433
echo - Keycloak: http://localhost:8080
echo - Web App: http://localhost:5000
echo.
echo To view logs: docker-compose logs -f
echo To stop: docker-compose down
pause
