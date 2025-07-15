#!/bin/bash

# Script to update client post-logout redirect URIs
echo "🔧 Updating client post-logout redirect URIs..."

# Wait until Keycloak is ready
echo "⏳ Waiting for Keycloak..."
until curl -s http://localhost:8082/health/ready > /dev/null; do
    echo "Keycloak not ready yet, waiting 5 seconds..."
    sleep 5
done

echo "✅ Keycloak is ready!"

# Get admin token
echo "🔑 Getting admin token..."
ADMIN_TOKEN=$(curl -s -X POST \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin&password=admin123&grant_type=password&client_id=admin-cli" \
  http://localhost:8082/realms/master/protocol/openid-connect/token | \
  jq -r '.access_token')

if [ "$ADMIN_TOKEN" = "null" ] || [ -z "$ADMIN_TOKEN" ]; then
    echo "❌ Failed to get admin token"
    exit 1
fi

echo "✅ Admin token obtained"

# Get client UUID
echo "🔍 Finding client UUID..."
CLIENT_UUID=$(curl -s -H "Authorization: Bearer $ADMIN_TOKEN" \
  http://localhost:8082/admin/realms/systeminstaller/clients?clientId=systeminstaller-client | \
  jq -r '.[0].id')

if [ "$CLIENT_UUID" = "null" ] || [ -z "$CLIENT_UUID" ]; then
    echo "❌ Client not found"
    exit 1
fi

echo "✅ Client UUID: $CLIENT_UUID"

# Get current client configuration
echo "📋 Getting current client configuration..."
CLIENT_CONFIG=$(curl -s -H "Authorization: Bearer $ADMIN_TOKEN" \
  http://localhost:8082/admin/realms/systeminstaller/clients/$CLIENT_UUID)

# Update client with post-logout redirect URIs
echo "🔧 Updating client with post-logout redirect URIs..."
curl -s -X PUT \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "'$CLIENT_UUID'",
    "clientId": "systeminstaller-client",
    "name": "SystemInstaller Web Application",
    "enabled": true,
    "protocol": "openid-connect",
    "publicClient": false,
    "standardFlowEnabled": true,
    "implicitFlowEnabled": false,
    "directAccessGrantsEnabled": true,
    "serviceAccountsEnabled": false,
    "authorizationServicesEnabled": false,
    "redirectUris": [
      "http://localhost:5000/*",
      "https://localhost:5000/*",
      "http://host.docker.internal:5000/*",
      "https://host.docker.internal:5000/*"
    ],
    "postLogoutRedirectUris": [
      "http://localhost:5000/",
      "https://localhost:5000/",
      "http://host.docker.internal:5000/",
      "https://host.docker.internal:5000/"
    ],
    "webOrigins": [
      "http://localhost:5000",
      "https://localhost:5000",
      "http://host.docker.internal:5000",
      "https://host.docker.internal:5000"
    ],
    "defaultClientScopes": [
      "web-origins",
      "role_list",
      "profile",
      "roles",
      "email"
    ],
    "optionalClientScopes": [
      "address",
      "phone",
      "offline_access",
      "microprofile-jwt"
    ]
  }' \
  http://localhost:8082/admin/realms/systeminstaller/clients/$CLIENT_UUID

echo "✅ Client updated with post-logout redirect URIs"

# Verify the update
echo "🔍 Verifying post-logout redirect URIs..."
UPDATED_CLIENT=$(curl -s -H "Authorization: Bearer $ADMIN_TOKEN" \
  http://localhost:8082/admin/realms/systeminstaller/clients/$CLIENT_UUID | \
  jq -r '.attributes."post.logout.redirect.uris"')

echo "Post-logout redirect URIs: $UPDATED_CLIENT"

echo "🎉 Client configuration updated successfully!"
