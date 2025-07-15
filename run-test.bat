@echo off
echo Starting Playwright test setup...

cd /d "d:\proj\SystemInstaller"
echo Current directory: %CD%

echo Checking Node.js...
node --version
if %ERRORLEVEL% NEQ 0 (
    echo Node.js not found!
    pause
    exit /b 1
)

echo Installing Playwright...
npm install playwright

echo Installing Playwright browsers...
npx playwright install chromium

echo Running browser test...
node test-browser.js

echo Running complete logout test...
node complete-logout-test.js

echo Test completed!
pause
