# Keycloak Authentication - Now Active!

This guide explains the activated Keycloak authentication integration for the SystemInstaller application.

## Overview

The SystemInstaller now includes **ACTIVE Keycloak authentication**:
- **API Gateway** (YARP-based) with JWT authentication on port 8090
- **React Frontend** with Keycloak integration on port 3000
- **Keycloak Server** for authentication on port 8082

## Architecture

```
React Frontend (3000) → API Gateway (8090) → [Protected APIs]
                            ↓
                      Keycloak (8082) [ACTIVE]
```

## What's Been Activated

### 1. Real Keycloak Integration
- ✅ **keycloak-js**: Added to package.json for React integration
- ✅ **Dynamic Import**: Real Keycloak with fallback to mock authentication
- ✅ **Silent SSO**: Silent check SSO file for seamless authentication
- ✅ **AuthProvider**: Wraps entire React app with authentication context

### 2. Gateway Authentication
- ✅ **JWT Bearer**: Added back JWT authentication to Gateway
- ✅ **Keycloak Integration**: Gateway validates tokens from Keycloak
- ✅ **Protected Endpoints**: Sample `/api/user` endpoint requires authentication
- ✅ **Public Endpoints**: `/health`, `/gateway/info`, `/api/public` remain public

### 3. React App Protection
- ✅ **Protected Routes**: All dashboard routes now require authentication
- ✅ **Auth Context**: Full authentication state management
- ✅ **Login/Logout**: Real Keycloak login/logout flow
- ✅ **Token Management**: Automatic token refresh and validation

## Prerequisites

1. **Node.js and npm** must be installed for React frontend dependencies
2. **Docker and Docker Compose** for container orchestration

## Setup Instructions

### 1. Install Frontend Dependencies (Required)

```bash
cd Presentation/Web
npm install
# The keycloak-js package is now included in package.json
```

### 2. Start Services

```bash
# Start Keycloak first (required for authentication)
docker-compose up -d keycloak

# Wait for Keycloak to be healthy, then start other services
docker-compose up -d gateway web
```

### 3. Configure Keycloak

1. **Access Keycloak Admin Console**: http://localhost:8082
2. **Login**: admin/admin123
3. **Verify Realm**: The "systeminstaller" realm should be auto-imported
4. **Test Users** are pre-configured:
   - **admin/admin123** (admin, user roles)
   - **user/user123** (user role)

### 4. Test Authentication Flow

1. **Open React Frontend**: http://localhost:3000
2. **Automatic Redirect**: App will redirect to Keycloak login
3. **Login**: Use admin/admin123 or user/user123
4. **Dashboard Access**: After login, access the protected dashboard

## API Endpoints

### Public Endpoints (No Authentication)
- `GET /health` - Gateway health check
- `GET /gateway/info` - Gateway information with Keycloak config
- `GET /api/public` - Sample public API endpoint

### Protected Endpoints (Requires Authentication)
- `GET /api/user` - Returns user information from JWT token

### Example API Usage

```bash
# Get public endpoint (no token needed)
curl http://localhost:8090/api/public

# Get protected endpoint (requires token)
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" http://localhost:8090/api/user
```

## Authentication Flow

1. **User accesses React app** → http://localhost:3000
2. **AuthProvider initializes** → Checks for existing Keycloak session
3. **No session found** → Redirects to Keycloak login
4. **User logs in** → Keycloak validates credentials
5. **JWT token received** → App stores token and user info
6. **Protected routes accessible** → Dashboard and all protected pages
7. **API calls authenticated** → Bearer token included in requests

## Troubleshooting

### Common Issues

1. **Keycloak Not Starting**
   ```bash
   # Check Keycloak logs
   docker logs systeminstaller-keycloak
   
   # Restart Keycloak
   docker-compose restart keycloak
   ```

2. **Authentication Failures**
   ```bash
   # Check Gateway logs for JWT validation errors
   docker logs systeminstaller-gateway
   
   # Verify Keycloak configuration
   curl http://localhost:8090/gateway/info
   ```

3. **Frontend Not Redirecting**
   - Ensure keycloak-js is installed: `npm install` in Presentation/Web
   - Check browser console for errors
   - Verify Keycloak is running and accessible

4. **CORS Errors**
   - Verify Gateway CORS configuration includes your frontend URL
   - Check that requests are going through Gateway (port 8090)

### Development Mode

If Keycloak is not available, the app will automatically fall back to mock authentication:
- Mock user data is used
- No real authentication occurs
- Useful for frontend development without infrastructure

## Production Considerations

1. **HTTPS**: Enable HTTPS for all services
2. **Real Secrets**: Replace development secrets with secure ones
3. **Token Expiry**: Configure appropriate token lifespans
4. **Realm Security**: Secure Keycloak realm configuration
5. **Rate Limiting**: Add rate limiting to Gateway

## Files Modified for Keycloak Activation

- `Presentation/Web/package.json` - Added keycloak-js dependency
- `Presentation/Web/src/services/keycloak.ts` - Real Keycloak integration
- `Presentation/Web/src/App.tsx` - AuthProvider and ProtectedRoute
- `Presentation/Web/public/silent-check-sso.html` - Silent SSO support
- `Gateway/Program.cs` - JWT authentication and protected endpoints
- `Gateway/SystemInstaller.Gateway.csproj` - JWT Bearer package

The SystemInstaller now has **full Keycloak authentication** activated and ready for use!
