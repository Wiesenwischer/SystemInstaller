@echo off
echo 🔧 Setting up Keycloak test user for SystemInstaller
echo ==================================================

REM Keycloak admin credentials (from docker-compose.yml)
set KEYCLOAK_URL=http://localhost:8082
set ADMIN_USER=admin
set ADMIN_PASS=admin
set REALM=systeminstaller

REM Test user details
set TEST_USER=testuser
set TEST_PASS=password
set TEST_EMAIL=testuser@systeminstaller.local

echo 🔄 Step 1: Getting admin access token...

REM Get admin access token
curl -s -X POST "%KEYCLOAK_URL%/realms/master/protocol/openid-connect/token" -H "Content-Type: application/x-www-form-urlencoded" -d "grant_type=password&client_id=admin-cli&username=%ADMIN_USER%&password=%ADMIN_PASS%" > token_response.json

if errorlevel 1 (
    echo ❌ Failed to connect to Keycloak. Is it running on %KEYCLOAK_URL%?
    del token_response.json 2>nul
    pause
    exit /b 1
)

echo ✅ Admin access token request sent

echo 🔄 Step 2: Creating test user directly via Keycloak admin console...
echo.
echo 📋 MANUAL SETUP INSTRUCTIONS:
echo ================================
echo.
echo 1. Open your browser to: %KEYCLOAK_URL%/admin/
echo 2. Login with:
echo    Username: %ADMIN_USER%
echo    Password: %ADMIN_PASS%
echo.
echo 3. Select the "%REALM%" realm from the dropdown
echo.
echo 4. Go to "Users" in the left menu
echo.
echo 5. Click "Create new user"
echo.
echo 6. Fill in the form:
echo    Username: %TEST_USER%
echo    Email: %TEST_EMAIL%
echo    First name: Test
echo    Last name: User
echo    Email verified: ON
echo    Enabled: ON
echo.
echo 7. Click "Create"
echo.
echo 8. Go to "Credentials" tab
echo.
echo 9. Click "Set password"
echo.
echo 10. Set password: %TEST_PASS%
echo     Temporary: OFF
echo.
echo 11. Click "Save"
echo.
echo ✅ After setup, you can test login at: %KEYCLOAK_URL%/realms/%REALM%/account
echo.
echo 🧪 Test credentials for logout tests:
echo    Username: %TEST_USER%
echo    Password: %TEST_PASS%
echo.

del token_response.json 2>nul
pause
