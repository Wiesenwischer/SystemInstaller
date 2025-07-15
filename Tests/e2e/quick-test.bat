@echo off
echo ================================
echo SystemInstaller Logout Test
echo ================================
echo.

echo 🔍 Step 1: Check if gateway is running
curl -s http://localhost:5000/health
if errorlevel 1 (
    echo ❌ Gateway not responding on localhost:5000
    echo Please ensure docker-compose up -d is running
    pause
    exit /b 1
)

echo.
echo ✅ Gateway is running

echo.
echo 🔍 Step 2: Test authentication endpoint (should be 401 if not logged in)
curl -s -o nul -w "Auth endpoint status: %%{http_code}\n" http://localhost:5000/auth/user

echo.
echo 🔍 Step 3: Test logout endpoint directly (should redirect to Keycloak)
curl -s -I http://localhost:5000/auth/logout | findstr "Location\|HTTP"

echo.
echo 🔍 Step 4: Check gateway logs for logout debug messages
echo Looking for logout debug messages in last 20 log lines:
docker logs systeminstaller-gateway --tail 20 | findstr -i "logout"

echo.
echo 📋 Manual Test Instructions:
echo 1. Open browser to http://localhost:5000
echo 2. Login with your credentials
echo 3. Click logout button
echo 4. Hit F5 (refresh)
echo 5. Check if you see:
echo    ✅ Login page (CORRECT) 
echo    ❌ Dashboard/homepage (BUG - automatic re-authentication)
echo.

pause
