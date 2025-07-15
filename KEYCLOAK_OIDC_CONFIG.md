# ReadyStackGo (RSGO) Keycloak OIDC Configuration

<div align="center">
  <img src="assets/logo.png" alt="ReadyStackGo Logo" width="300">
</div>

> **Turn your specs into stacks**

## ✅ What We've Updated

### 1. **Keycloak Realm Configuration** (`keycloak/readystackgo-realm.json`)
- **Redirect URIs**: Updated to `http://localhost:5000/signin-oidc` (Gateway OIDC callback)
- **Web Origins**: Added `http://localhost:5000` (Gateway) and kept `http://localhost:3000` (React)
- **Post Logout Redirect URIs**: Added both Gateway and React URLs
- **Client Type**: Changed from `publicClient: true` to `publicClient: false` (confidential client)
- **Client Secret**: Using `development-secret`
- **Direct Access Grants**: Disabled (no longer needed for OIDC flow)
- **Front Channel Logout**: Enabled for proper logout flow

### 2. **Gateway Configuration** (`Gateway/appsettings.json`)
- **ClientSecret**: Updated to match realm configuration (`development-secret`)
- **Keycloak URL**: Configured for `http://localhost:8082`
- **Realm**: `readystackgo`
- **ClientId**: `readystackgo-client`

### 3. **Setup Scripts**
- **`setup-keycloak-oidc.ps1`**: Windows PowerShell script to import realm
- **`setup-keycloak-oidc.sh`**: Linux/Mac bash script to import realm
- Both scripts handle existing realm deletion and verification

## 🔧 Key Configuration Changes

### Before (PKCE Frontend Auth)
```json
{
  "redirectUris": ["http://localhost:3000/*"],
  "webOrigins": ["http://localhost:3000"],
  "publicClient": true,
  "directAccessGrantsEnabled": true
}
```

### After (Gateway OIDC Auth)
```json
{
  "redirectUris": [
    "http://localhost:5000/signin-oidc",
    "http://localhost:5000/*"
  ],
  "webOrigins": [
    "http://localhost:5000",
    "http://localhost:3000"
  ],
  "publicClient": false,
  "directAccessGrantsEnabled": false,
  "clientAuthenticatorType": "client-secret",
  "secret": "development-secret"
}
```

## 🚀 How to Use

### 1. Start Keycloak
```powershell
docker run -p 8082:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin123 quay.io/keycloak/keycloak:latest start-dev
```

### 2. Import Realm Configuration
```powershell
# Windows
.\setup-keycloak-oidc.ps1

# Linux/Mac
./setup-keycloak-oidc.sh
```

### 3. Start Gateway
```powershell
cd Gateway
dotnet run
```

### 4. Test Authentication
- Open browser: `http://localhost:5000/auth/login`
- Should redirect to Keycloak login page
- Login with: `admin/admin123` or `customer/customer123`
- Should redirect back to Gateway

## 🧪 Test Users
- **Admin User**: `admin` / `admin123` (has admin role)
- **Customer User**: `customer` / `customer123` (has customer role)

## 📋 Authentication Flow

```
1. User visits protected resource
2. Gateway detects unauthenticated user
3. Gateway redirects to Keycloak login page
4. User enters credentials in Keycloak
5. Keycloak redirects to Gateway with authorization code
6. Gateway exchanges code for tokens with Keycloak
7. Gateway sets secure HTTP-only authentication cookies
8. User can access protected resources
```

## 🔍 Verification Steps

### Check Realm Import
1. Open Keycloak Admin: `http://localhost:8082`
2. Login with `admin/admin123`
3. Switch to `systeminstaller` realm
4. Go to Clients → `systeminstaller-client`
5. Verify redirect URIs include `http://localhost:5000/signin-oidc`

### Test OIDC Flow
1. Gateway running on port 5000
2. Visit: `http://localhost:5000/auth/login`
3. Should redirect to: `http://localhost:8082/realms/systeminstaller/protocol/openid-connect/auth?...`
4. After login, should redirect back to Gateway

### Check Authentication
1. Visit: `http://localhost:5000/auth/user`
2. Should show user information if authenticated
3. Should return 401 if not authenticated

## 🔧 Troubleshooting

### "Invalid redirect URI"
- Check Keycloak client configuration
- Ensure `http://localhost:5000/signin-oidc` is in Valid Redirect URIs

### "Client authentication failed"
- Verify client secret matches in both Keycloak and Gateway config
- Check that client is set to confidential (not public)

### "404 Not Found" on Gateway
- Ensure Gateway is running on port 5000
- Check Gateway console for startup errors

### Frontend Not Working
- React app should still work on port 3000
- Gateway will proxy requests to React app
- Authentication is handled by Gateway, not React

The Keycloak realm is now properly configured for Gateway OIDC authentication!
