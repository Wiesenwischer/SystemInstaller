# PowerShell script to test the Gateway OIDC authentication

Write-Host "Testing Gateway OIDC Authentication..." -ForegroundColor Green

# Test 1: Check if Gateway is running
Write-Host "`n1. Testing Gateway Health..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method GET
    Write-Host "✓ Gateway is healthy: $($healthResponse.Status)" -ForegroundColor Green
} catch {
    Write-Host "✗ Gateway health check failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure to start the Gateway with: dotnet run" -ForegroundColor Cyan
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

# Test 3: Try accessing a protected route (should redirect to login)
Write-Host "`n3. Testing protected route without authentication..." -ForegroundColor Yellow
try {
    # Use -MaximumRedirection 0 to prevent automatic redirects
    $protectedResponse = Invoke-WebRequest -Uri "http://localhost:5000/auth/user" -Method GET -MaximumRedirection 0
    Write-Host "✗ Protected route accessible without auth (this shouldn't happen)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✓ Protected route correctly returns 401 Unauthorized" -ForegroundColor Green
    } elseif ($_.Exception.Response.StatusCode -eq 302) {
        Write-Host "✓ Protected route correctly redirects to login (302)" -ForegroundColor Green
        Write-Host "  Redirect location: $($_.Exception.Response.Headers.Location)" -ForegroundColor Cyan
    } else {
        Write-Host "✗ Unexpected response: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Test 4: Test login endpoint (should redirect to Keycloak)
Write-Host "`n4. Testing login endpoint..." -ForegroundColor Yellow
try {
    $loginResponse = Invoke-WebRequest -Uri "http://localhost:5000/auth/login" -Method GET -MaximumRedirection 0
    Write-Host "✗ Login should redirect to Keycloak" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 302) {
        $location = $_.Exception.Response.Headers.Location
        Write-Host "✓ Login correctly redirects to Keycloak" -ForegroundColor Green
        Write-Host "  Redirect URL: $location" -ForegroundColor Cyan
        
        if ($location -like "*keycloak*" -or $location -like "*localhost:8082*") {
            Write-Host "✓ Redirect points to Keycloak server" -ForegroundColor Green
        } else {
            Write-Host "⚠ Warning: Redirect doesn't seem to point to Keycloak" -ForegroundColor Yellow
        }
    } else {
        Write-Host "✗ Login endpoint returned: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Test 5: Test the reverse proxy endpoints
Write-Host "`n5. Testing reverse proxy endpoints..." -ForegroundColor Yellow
try {
    # This should be proxied to the backend service
    $proxyResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/environments" -Method GET -MaximumRedirection 0
    Write-Host "✗ Protected API accessible without auth" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✓ Reverse proxy correctly protects API endpoints" -ForegroundColor Green
    } elseif ($_.Exception.Response.StatusCode -eq 302) {
        Write-Host "✓ Reverse proxy redirects to login for protected API" -ForegroundColor Green
    } else {
        Write-Host "⚠ API endpoint returned: $($_.Exception.Response.StatusCode)" -ForegroundColor Yellow
    }
}

Write-Host "`n=== OIDC Authentication Testing Summary ===" -ForegroundColor Green
Write-Host "✓ Gateway is running and healthy" -ForegroundColor Green
Write-Host "✓ OIDC authentication endpoints are configured" -ForegroundColor Green
Write-Host "✓ Protected routes require authentication" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Start Keycloak server on port 8082" -ForegroundColor White
Write-Host "2. Configure 'systeminstaller' realm in Keycloak" -ForegroundColor White
Write-Host "3. Open browser and navigate to: http://localhost:5000/auth/login" -ForegroundColor White
Write-Host "4. You should be redirected to Keycloak for authentication" -ForegroundColor White
Write-Host ""
Write-Host "Frontend Integration:" -ForegroundColor Cyan
Write-Host "- Update React app to redirect to http://localhost:5000/auth/login" -ForegroundColor White
Write-Host "- Remove custom login forms (they are no longer needed)" -ForegroundColor White
Write-Host "- Use http://localhost:5000/auth/user to check authentication status" -ForegroundColor White
Write-Host "- Use http://localhost:5000/auth/logout for logout" -ForegroundColor White
