# PowerShell script to update client post-logout redirect URIs
Write-Host "🔧 Updating client post-logout redirect URIs..." -ForegroundColor Green

# Wait until Keycloak is ready
Write-Host "⏳ Waiting for Keycloak..." -ForegroundColor Yellow
do {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:8082/health/ready" -Method Get -TimeoutSec 5
        break
    } catch {
        Write-Host "Keycloak not ready yet, waiting 5 seconds..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    }
} while ($true)

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
    $tokenResponse = Invoke-RestMethod -Uri "http://localhost:8082/realms/master/protocol/openid-connect/token" -Method Post -Body $tokenBody -ContentType "application/x-www-form-urlencoded"
    $adminToken = $tokenResponse.access_token
    Write-Host "✅ Admin token obtained" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to get admin token: $_" -ForegroundColor Red
    exit 1
}

# Get client UUID
Write-Host "🔍 Finding client UUID..." -ForegroundColor Yellow
$headers = @{
    Authorization = "Bearer $adminToken"
}

try {
    $clientsResponse = Invoke-RestMethod -Uri "http://localhost:8082/admin/realms/systeminstaller/clients?clientId=systeminstaller-client" -Method Get -Headers $headers
    $clientUuid = $clientsResponse[0].id
    Write-Host "✅ Client UUID: $clientUuid" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to find client: $_" -ForegroundColor Red
    exit 1
}

# Update client with post-logout redirect URIs
Write-Host "🔧 Updating client with post-logout redirect URIs..." -ForegroundColor Yellow

$clientConfig = @{
    id = $clientUuid
    clientId = "systeminstaller-client"
    name = "SystemInstaller Web Application"
    enabled = $true
    protocol = "openid-connect"
    publicClient = $false
    standardFlowEnabled = $true
    implicitFlowEnabled = $false
    directAccessGrantsEnabled = $true
    serviceAccountsEnabled = $false
    authorizationServicesEnabled = $false
    redirectUris = @(
        "http://localhost:5000/*",
        "https://localhost:5000/*",
        "http://host.docker.internal:5000/*",
        "https://host.docker.internal:5000/*"
    )
    postLogoutRedirectUris = @(
        "http://localhost:5000/",
        "https://localhost:5000/",
        "http://host.docker.internal:5000/",
        "https://host.docker.internal:5000/"
    )
    webOrigins = @(
        "http://localhost:5000",
        "https://localhost:5000",
        "http://host.docker.internal:5000",
        "https://host.docker.internal:5000"
    )
    defaultClientScopes = @(
        "web-origins",
        "role_list",
        "profile",
        "roles",
        "email"
    )
    optionalClientScopes = @(
        "address",
        "phone",
        "offline_access",
        "microprofile-jwt"
    )
}

$clientConfigJson = $clientConfig | ConvertTo-Json -Depth 10

try {
    $updateResponse = Invoke-RestMethod -Uri "http://localhost:8082/admin/realms/systeminstaller/clients/$clientUuid" -Method Put -Headers $headers -Body $clientConfigJson -ContentType "application/json"
    Write-Host "✅ Client updated with post-logout redirect URIs" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to update client: $_" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
    exit 1
}

# Verify the update
Write-Host "🔍 Verifying post-logout redirect URIs..." -ForegroundColor Yellow
try {
    $verifyResponse = Invoke-RestMethod -Uri "http://localhost:8082/admin/realms/systeminstaller/clients/$clientUuid" -Method Get -Headers $headers
    Write-Host "Post-logout redirect URIs: $($verifyResponse.postLogoutRedirectUris -join ', ')" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Could not verify configuration: $_" -ForegroundColor Yellow
}

Write-Host "🎉 Client configuration updated successfully!" -ForegroundColor Green
