Write-Host "Testing logout flow..." -ForegroundColor Green

# Test 1: Check if app redirects to login
Write-Host "1. Testing app access (should redirect to login)..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/" -Method Get -MaximumRedirection 0 -ErrorAction SilentlyContinue
    Write-Host "Status: $($response.StatusCode) - $($response.Headers.Location)" -ForegroundColor Blue
} catch {
    Write-Host "Redirect detected: $($_.Exception.Response.Headers.Location)" -ForegroundColor Blue
}

# Test 2: Check logout endpoint
Write-Host "2. Testing logout endpoint..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/auth/logout" -Method Get -MaximumRedirection 0 -ErrorAction SilentlyContinue
    Write-Host "Status: $($response.StatusCode) - $($response.Headers.Location)" -ForegroundColor Blue
} catch {
    Write-Host "Redirect detected: $($_.Exception.Response.Headers.Location)" -ForegroundColor Blue
}

# Test 3: Check login endpoint
Write-Host "3. Testing login endpoint..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/auth/login" -Method Get -MaximumRedirection 0 -ErrorAction SilentlyContinue
    Write-Host "Status: $($response.StatusCode) - $($response.Headers.Location)" -ForegroundColor Blue
} catch {
    Write-Host "Redirect detected: $($_.Exception.Response.Headers.Location)" -ForegroundColor Blue
}

Write-Host "Test completed." -ForegroundColor Green
