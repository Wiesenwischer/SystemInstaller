@echo off
echo 🚀 Setting up SystemInstaller E2E Tests

REM Navigate to test directory
cd /d "%~dp0"

REM Install dependencies
echo 📦 Installing dependencies...
npm install

REM Install Playwright browsers
echo 🌐 Installing Playwright browsers...
npx playwright install

REM Create test credentials file
echo # Test configuration for SystemInstaller E2E tests > .env.test
echo TEST_BASE_URL=http://localhost:5000 >> .env.test
echo KEYCLOAK_URL=http://localhost:8082 >> .env.test
echo TEST_USERNAME=testuser >> .env.test
echo TEST_PASSWORD=password >> .env.test

echo ✅ Setup complete!
echo.
echo 📋 Available commands:
echo   npm test              - Run all tests headless
echo   npm run test:headed   - Run tests with browser UI
echo   npm run test:debug    - Run tests in debug mode
echo.
echo ⚠️  Make sure SystemInstaller is running on localhost:5000 before running tests
echo ⚠️  Make sure Keycloak is running on localhost:8082 before running tests

pause
