@echo off
cd /d "d:\proj\SystemInstaller"
echo Building Tailwind CSS...
call npm run build-css
echo Starting Blazor application...
dotnet run
pause
