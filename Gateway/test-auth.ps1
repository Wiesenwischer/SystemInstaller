# PowerShell script to test the Gateway authentication

Write-Host "Testing Gateway Authentication..." -ForegroundColor Green

# Test 1: Check if Gateway is running
Write-Host "`n1. Testing Gateway Health..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method GET
    Write-Host "✓ Gateway is healthy: $($healthResponse.Status)" -ForegroundColor Green
} catch {
    Write-Host "✗ Gateway health check failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Get Gateway info
Write-Host "`n2. Getting Gateway info..." -ForegroundColor Yellow
try {
    $infoResponse = Invoke-RestMethod -Uri "http://localhost:5000/gateway/info" -Method GET
    Write-Host "✓ Gateway Info:" -ForegroundColor Green
    Write-Host "  Version: $($infoResponse.Version)"
    Write-Host "  Environment: $($infoResponse.Environment)"
    Write-Host "  Keycloak URL: $($infoResponse.Keycloak.Url)"
    Write-Host "  Keycloak Realm: $($infoResponse.Keycloak.Realm)"
} catch {
    Write-Host "✗ Gateway info failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Try accessing a protected route (should fail)
Write-Host "`n3. Testing protected route without authentication..." -ForegroundColor Yellow
try {
    $protectedResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/environments" -Method GET
    Write-Host "✗ Protected route accessible without auth (this shouldn't happen)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✓ Protected route correctly returns 401 Unauthorized" -ForegroundColor Green
    } else {
        Write-Host "✗ Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 4: Test login endpoint (will fail without Keycloak running)
Write-Host "`n4. Testing login endpoint..." -ForegroundColor Yellow
$loginData = @{
    username = "test@example.com"
    password = "test123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/auth/login" -Method POST -Body $loginData -ContentType "application/json"
    Write-Host "✓ Login successful" -ForegroundColor Green
} catch {
    Write-Host "✗ Login failed (expected if Keycloak is not running): $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`nAuthentication testing completed!" -ForegroundColor Green
Write-Host "Note: Full authentication will work once Keycloak is running and configured." -ForegroundColor Cyan
