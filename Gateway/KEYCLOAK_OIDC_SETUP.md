# Keycloak Configuration for OIDC Authentication

## 🔧 Required Keycloak Configuration

Since we switched from custom authentication to proper OpenID Connect (OIDC), Keycloak needs to be reconfigured to support the standard OIDC Authorization Code flow.

## 📋 Configuration Steps

### 1. Update Gateway Configuration

First, update the Gateway's appsettings.json:

```json
{
  "Keycloak": {
    "Url": "http://localhost:8082",
    "Realm": "systeminstaller",
    "ClientId": "systeminstaller-client",
    "ClientSecret": "your-actual-client-secret"
  }
}
```

### 2. Keycloak Realm Configuration

#### Create/Verify Realm
1. **Access Keycloak Admin Console**: http://localhost:8082/admin
2. **Create Realm** (if not exists):
   - Name: `systeminstaller`
   - Enabled: Yes

### 3. Client Configuration (CRITICAL CHANGES)

#### Create/Update Client
1. **Navigate to**: Realm `systeminstaller` → Clients
2. **Create new client** or **edit existing**:

#### General Settings
- **Client ID**: `systeminstaller-client`
- **Name**: `SystemInstaller Gateway Client`
- **Description**: `OIDC client for SystemInstaller Gateway`
- **Enabled**: On

#### Access Settings (IMPORTANT!)
- **Client authentication**: On (this makes it a confidential client)
- **Authorization**: Off
- **Authentication flow**:
  - ✅ Standard flow (Authorization Code Flow)
  - ❌ Direct access grants (disable for security)
  - ❌ Implicit flow (disable for security)
  - ❌ Service accounts roles (not needed)

#### Login Settings
- **Root URL**: `http://localhost:5000`
- **Home URL**: `http://localhost:5000`
- **Valid redirect URIs**: 
  ```
  http://localhost:5000/signin-oidc
  http://localhost:5000/
  ```
- **Valid post logout redirect URIs**:
  ```
  http://localhost:5000/
  http://localhost:5000/signout-callback-oidc
  ```
- **Web origins**: `http://localhost:5000`

#### Advanced Settings
- **Access Token Lifespan**: 24 hours (or as needed)
- **Client Session Idle**: 30 minutes
- **Client Session Max**: 24 hours

### 4. Client Secret Configuration

#### Get Client Secret
1. **Go to**: Client → Credentials tab
2. **Copy the Secret**: This goes in appsettings.json
3. **Update appsettings.json**:
   ```json
   "ClientSecret": "paste-actual-secret-here"
   ```

### 5. User Configuration

#### Create Test Users
1. **Navigate to**: Realm `systeminstaller` → Users
2. **Create user**:
   - Username: `testuser`
   - Email: `test@systeminstaller.local`
   - First Name: `Test`
   - Last Name: `User`
   - Email Verified: On
   - Enabled: On

#### Set Password
1. **Go to**: User → Credentials tab
2. **Set password**: Choose a password
3. **Temporary**: Off (so user doesn't need to change on first login)

### 6. Roles Configuration (Optional)

#### Create Application Roles
1. **Navigate to**: Clients → `systeminstaller-client` → Roles
2. **Create roles**:
   - `admin`
   - `user`
   - `viewer`

#### Assign Roles to Users
1. **Go to**: Users → Select user → Role mapping
2. **Assign client roles** as needed

## 🔍 Important OIDC Endpoints

After configuration, these endpoints will be available:

### Keycloak OIDC Endpoints
```
# Discovery document
http://localhost:8082/realms/systeminstaller/.well-known/openid-configuration

# Authorization endpoint
http://localhost:8082/realms/systeminstaller/protocol/openid-connect/auth

# Token endpoint  
http://localhost:8082/realms/systeminstaller/protocol/openid-connect/token

# User info endpoint
http://localhost:8082/realms/systeminstaller/protocol/openid-connect/userinfo

# Logout endpoint
http://localhost:8082/realms/systeminstaller/protocol/openid-connect/logout
```

### Gateway Endpoints
```
# Start login (redirects to Keycloak)
http://localhost:5000/auth/login

# Logout
http://localhost:5000/auth/logout

# Get current user info
http://localhost:5000/auth/user

# OIDC callback (used by Keycloak)
http://localhost:5000/signin-oidc
```

## 🚀 Testing the Configuration

### 1. Start Keycloak
```bash
# If using Docker
docker run -p 8082:8080 \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:latest start-dev

# Or if using standalone installation
./bin/kc.sh start-dev --http-port=8082
```

### 2. Start Gateway
```bash
cd d:\proj\SystemInstaller\Gateway
dotnet run
```

### 3. Test Authentication Flow
1. **Open browser**: http://localhost:5000/auth/login
2. **Should redirect to**: Keycloak login page
3. **Login with test user**
4. **Should redirect back to**: Gateway
5. **Check user info**: http://localhost:5000/auth/user

## ❌ Common Configuration Errors

### 1. Invalid Redirect URI
**Error**: "Invalid parameter: redirect_uri"
**Solution**: Ensure `http://localhost:5000/signin-oidc` is in Valid redirect URIs

### 2. Client Authentication Failed
**Error**: "Client authentication failed"
**Solution**: 
- Verify ClientId matches exactly
- Check ClientSecret is correct
- Ensure "Client authentication" is enabled

### 3. CORS Issues
**Error**: CORS policy blocks requests
**Solution**: Add Gateway URL to "Web origins" in Keycloak client

### 4. Token Validation Failed
**Error**: "Invalid token"
**Solution**: 
- Check realm name matches
- Verify client configuration
- Ensure user has required roles

## 🔄 Migration from Old System

### What Changed
- ❌ **Old**: Custom login forms with username/password
- ✅ **New**: Redirect to Keycloak login page

- ❌ **Old**: Manual token management
- ✅ **New**: Automatic OIDC token handling

- ❌ **Old**: Custom middleware for authentication
- ✅ **New**: Built-in ASP.NET Core OIDC middleware

### Frontend Updates Needed
```typescript
// Old approach (remove this)
await fetch('/api/auth/login', {
  method: 'POST',
  body: JSON.stringify({ username, password })
});

// New approach (use this)
window.location.href = '/auth/login'; // Redirects to Keycloak
```

## ✅ Verification Checklist

- [ ] Keycloak running on port 8082
- [ ] Realm `systeminstaller` created
- [ ] Client `systeminstaller-client` configured
- [ ] Client secret copied to appsettings.json
- [ ] Valid redirect URIs configured
- [ ] Test user created with password
- [ ] Gateway starts without errors
- [ ] `/auth/login` redirects to Keycloak
- [ ] Login with test user works
- [ ] `/auth/user` returns user information
- [ ] `/auth/logout` signs out properly

## 🎯 Next Steps

1. **Complete Keycloak setup** using this guide
2. **Test authentication flow** manually
3. **Update frontend** to use OIDC redirects
4. **Remove old authentication components**
5. **Test complete application flow**

The new OIDC approach is much more secure and standards-compliant than our previous custom implementation!
