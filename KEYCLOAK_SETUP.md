# Simplified Gateway + Frontend Architecture

This guide explains the simplified SystemInstaller architecture with only Gateway and Frontend components.

## Overview

The SystemInstaller now includes:
- **API Gateway** (YARP-based) for routing and CORS on port 8090
- **React Frontend** with TailAdmin template on port 3000
- **Keycloak Server** for authentication (optional) on port 8082

## Simplified Architecture

```
React Frontend (3000) → API Gateway (8090)
                            ↓
                      Keycloak (8082) [Optional]
```

## Current Components

### 1. API Gateway (Port 8090)
- **YARP Reverse Proxy**: Routes all traffic to React frontend
- **Health Endpoints**: `/health` and `/gateway/info`
- **CORS Support**: Configured for React frontend
- **Simplified**: No authentication or backend API routing

### 2. React Frontend (Port 3000)
- **TailAdmin Template**: Modern admin interface
- **Vite + React + TypeScript**: Modern development stack
- **Tailwind CSS 4.0**: Utility-first styling
- **Nginx Container**: Production-ready serving

### 3. Keycloak (Port 8082) [Optional]
- **Identity Provider**: Ready for future authentication needs
- **Realm Configuration**: Pre-configured systeminstaller realm
- **Test Users**: admin/admin123 and user/user123

## Prerequisites

1. **Node.js and npm** must be installed for React frontend dependencies
2. **Docker and Docker Compose** for container orchestration

## Setup Instructions

### 1. Install Frontend Dependencies

```bash
cd Presentation/Web
npm install
npm install keycloak-js
```

### 2. Update Keycloak Realm Configuration

The realm configuration is in `keycloak/systeminstaller-realm.json`. Update the client redirect URIs if needed:

```json
"redirectUris": [
  "http://localhost:3000/*",
  "http://localhost:8090/*"
],
"webOrigins": [
  "http://localhost:3000",
  "http://localhost:8090"
]
```

### 3. Start the Services

```bash
# Start all services
docker-compose up -d

# Or start individual services
docker-compose up -d keycloak
docker-compose up -d db
docker-compose up -d api
docker-compose up -d gateway
docker-compose up -d web
```

### 4. Configure Keycloak

1. Access Keycloak Admin Console: http://localhost:8082
2. Login with admin/admin123
3. The realm "systeminstaller" should be auto-imported
4. Test users are pre-configured:
   - **admin/admin123** (admin, user roles)
   - **user/user123** (user role)

### 5. Test Authentication Flow

1. Open React frontend: http://localhost:3000
2. The app will automatically redirect to Keycloak login
3. Login with test credentials
4. Navigate through protected routes

## Files Created/Modified

### API Gateway
- `Gateway/SystemInstaller.Gateway.csproj` - YARP and JWT packages
- `Gateway/Program.cs` - Gateway configuration with authentication
- `Gateway/appsettings.json` - YARP routing and Keycloak config
- `Dockerfile.gateway` - Gateway containerization

### React Frontend Authentication
- `src/services/keycloak.ts` - Keycloak service integration
- `src/contexts/AuthContext.tsx` - React authentication context
- `src/components/ProtectedRoute.tsx` - Route protection component
- `src/components/UserProfile.tsx` - User profile display
- `src/services/api.ts` - API client with JWT headers

### Configuration
- `docker-compose.yml` - Added Gateway service
- `keycloak/systeminstaller-realm.json` - Keycloak realm configuration

## Mock Authentication (Development)

For development without Keycloak running, the frontend includes a mock authentication system:
- Uses localStorage to simulate login state
- Provides mock user data and roles
- Can be toggled by modifying `src/services/keycloak.ts`

## API Gateway Routes

The Gateway handles routing:
- `/api/*` → Backend API (with authentication required)
- `/*` → React Frontend (public access)
- `/health` → Gateway health check
- `/gateway/info` → Gateway information

## Security Features

1. **JWT Token Validation** - All API requests require valid JWT tokens
2. **Role-Based Access Control** - Different endpoints can require specific roles
3. **CORS Configuration** - Properly configured for frontend-gateway communication
4. **Token Refresh** - Automatic token refresh before expiry
5. **Secure Headers** - Security headers added by Gateway

## Production Considerations

1. **HTTPS** - Enable HTTPS for all services in production
2. **Secrets Management** - Use proper secret management for client secrets
3. **Token Expiry** - Configure appropriate token lifespans
4. **Rate Limiting** - Add rate limiting to the Gateway
5. **Monitoring** - Add logging and monitoring for authentication events

## Troubleshooting

### Common Issues

1. **CORS Errors**: Check Gateway CORS configuration
2. **Authentication Failures**: Verify Keycloak realm and client configuration
3. **Token Expiry**: Check token refresh logic in React app
4. **Network Issues**: Ensure all services can communicate

### Useful Commands

```bash
# Check Gateway logs
docker logs systeminstaller-gateway

# Check Keycloak logs
docker logs systeminstaller-keycloak

# Test Gateway health
curl http://localhost:8090/health

# Test API through Gateway
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:8090/api/environments
```

## Next Steps

1. Complete the React frontend by updating existing components to use the AuthContext
2. Add role-based UI elements (admin-only buttons, etc.)
3. Implement proper error handling for authentication failures
4. Add user profile management features
5. Configure production-ready Keycloak settings

## Development Notes

The current implementation provides a foundation for:
- Microservices architecture with API Gateway
- Centralized authentication with Keycloak
- JWT-based API security
- Role-based access control
- Scalable frontend-backend communication

The mock authentication allows development to continue while Keycloak is being set up, and can be easily replaced with real Keycloak integration once the infrastructure is ready.
