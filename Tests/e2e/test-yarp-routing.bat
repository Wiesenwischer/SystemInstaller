@echo off
echo 🚀 Running YARP Routing Configuration Tests
echo ===========================================
echo.

echo 📍 Current directory: %CD%
echo 🔍 Testing YARP routing configuration...
echo.

REM Run the YARP routing test
node yarp-routing-test.js

echo.
echo ✅ YARP routing tests completed!
echo.
echo 💡 If auth routes are being proxied incorrectly, the YARP configuration needs adjustment.
echo 💡 If frontend routes are not being proxied, check the YARP cluster configuration.
pause
