# PowerShell script to setup Keycloak for SystemInstaller OIDC Gateway Authentication

Write-Host "🚀 SystemInstaller Keycloak OIDC Setup" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green

# Check if Keycloak is running
Write-Host "🔍 Checking if Keycloak is running..." -ForegroundColor Yellow
try {
    $healthCheck = Invoke-RestMethod -Uri "http://localhost:8082/health" -TimeoutSec 5
    Write-Host "✅ Keycloak is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Keycloak is not running on port 8082" -ForegroundColor Red
    Write-Host "Please start Keycloak first:" -ForegroundColor Cyan
    Write-Host "  docker run -p 8082:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin123 quay.io/keycloak/keycloak:latest start-dev" -ForegroundColor White
    exit 1
}

# Wait for Keycloak to be ready
Write-Host "⏳ Waiting for Keycloak to be ready..." -ForegroundColor Yellow
do {
    try {
        $readyCheck = Invoke-RestMethod -Uri "http://localhost:8082/health/ready" -TimeoutSec 5
        $ready = $true
        break
    } catch {
        Write-Host "Keycloak is not ready yet, waiting 5 seconds..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
        $ready = $false
    }
} while (-not $ready)

Write-Host "✅ Keycloak is ready!" -ForegroundColor Green

# Get admin token
Write-Host "🔑 Getting admin token..." -ForegroundColor Yellow
$tokenBody = @{
    username = "admin"
    password = "admin123"
    grant_type = "password"
    client_id = "admin-cli"
}

try {
    $tokenResponse = Invoke-RestMethod -Uri "http://localhost:8082/realms/master/protocol/openid-connect/token" `
        -Method POST -Body $tokenBody -ContentType "application/x-www-form-urlencoded"
    $adminToken = $tokenResponse.access_token
    Write-Host "✅ Admin token obtained" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to get admin token" -ForegroundColor Red
    Write-Host "Make sure Keycloak admin credentials are: admin/admin123" -ForegroundColor Cyan
    exit 1
}

# Check if realm already exists
Write-Host "🔍 Checking if systeminstaller realm exists..." -ForegroundColor Yellow
$headers = @{ Authorization = "Bearer $adminToken" }
try {
    $existingRealm = Invoke-RestMethod -Uri "http://localhost:8082/admin/realms/systeminstaller" -Headers $headers
    Write-Host "⚠️  Realm 'systeminstaller' already exists" -ForegroundColor Yellow
    
    $choice = Read-Host "Do you want to delete and recreate it? (y/N)"
    if ($choice -eq 'y' -or $choice -eq 'Y') {
        Write-Host "🗑️  Deleting existing realm..." -ForegroundColor Yellow
        Invoke-RestMethod -Uri "http://localhost:8082/admin/realms/systeminstaller" -Method DELETE -Headers $headers
        Write-Host "✅ Existing realm deleted" -ForegroundColor Green
    } else {
        Write-Host "❌ Setup cancelled" -ForegroundColor Red
        exit 1
    }
} catch {
    # Realm doesn't exist, which is fine
    Write-Host "✅ Realm doesn't exist yet, will create new one" -ForegroundColor Green
}

# Import realm configuration
Write-Host "📥 Importing SystemInstaller realm configuration..." -ForegroundColor Yellow
$realmConfig = Get-Content -Path "keycloak\systeminstaller-realm.json" -Raw
$headers["Content-Type"] = "application/json"

try {
    Invoke-RestMethod -Uri "http://localhost:8082/admin/realms" -Method POST -Headers $headers -Body $realmConfig
    Write-Host "✅ Realm configuration imported successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to import realm configuration" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}

# Verify the configuration
Write-Host "🔍 Verifying realm configuration..." -ForegroundColor Yellow
try {
    $clients = Invoke-RestMethod -Uri "http://localhost:8082/admin/realms/systeminstaller/clients" -Headers @{ Authorization = "Bearer $adminToken" }
    $systemInstallerClient = $clients | Where-Object { $_.clientId -eq "systeminstaller-client" }
    
    if ($systemInstallerClient) {
        Write-Host "✅ Client configuration verified:" -ForegroundColor Green
        Write-Host "  Client ID: $($systemInstallerClient.clientId)" -ForegroundColor White
        Write-Host "  Enabled: $($systemInstallerClient.enabled)" -ForegroundColor White
        Write-Host "  Redirect URIs: $($systemInstallerClient.redirectUris -join ', ')" -ForegroundColor White
    } else {
        Write-Host "⚠️  Client not found, but realm was imported" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Could not verify client configuration, but realm was imported" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🎉 Keycloak setup completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Configuration Summary:" -ForegroundColor Cyan
Write-Host "  Realm: systeminstaller" -ForegroundColor White
Write-Host "  Client ID: systeminstaller-client" -ForegroundColor White
Write-Host "  Client Secret: development-secret" -ForegroundColor White
Write-Host "  Gateway URL: http://localhost:5000" -ForegroundColor White
Write-Host "  Keycloak URL: http://localhost:8082" -ForegroundColor White
Write-Host ""
Write-Host "🧪 Test Users:" -ForegroundColor Cyan
Write-Host "  Admin: admin / admin123" -ForegroundColor White
Write-Host "  Customer: customer / customer123" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Start the Gateway: cd Gateway; dotnet run" -ForegroundColor White
Write-Host "  2. Open browser: http://localhost:5000/auth/login" -ForegroundColor White
Write-Host "  3. You should be redirected to Keycloak login" -ForegroundColor White
Write-Host ""
