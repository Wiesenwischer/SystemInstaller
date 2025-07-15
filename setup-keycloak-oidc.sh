#!/bin/bash

# Keycloak Setup Script for SystemInstaller with OIDC Gateway Authentication
# This script imports the realm configuration for Gateway-based authentication

echo "🚀 SystemInstaller Keycloak OIDC Setup"
echo "======================================"

# Check if Keycloak is running
echo "🔍 Checking if Keycloak is running..."
if ! curl -s http://localhost:8082/health > /dev/null 2>&1; then
    echo "❌ Keycloak is not running on port 8082"
    echo "Please start Keycloak first:"
    echo "  docker run -p 8082:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin123 quay.io/keycloak/keycloak:latest start-dev"
    exit 1
fi

echo "✅ Keycloak is running"

# Wait for Keycloak to be ready
echo "⏳ Waiting for Keycloak to be ready..."
until curl -s http://localhost:8082/health/ready > /dev/null; do
    echo "Keycloak is not ready yet, waiting 5 seconds..."
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
    echo "Make sure Keycloak admin credentials are: admin/admin123"
    exit 1
fi

echo "✅ Admin token obtained"

# Check if realm already exists
echo "🔍 Checking if systeminstaller realm exists..."
REALM_EXISTS=$(curl -s -H "Authorization: Bearer $ADMIN_TOKEN" \
  http://localhost:8082/admin/realms/systeminstaller | jq -r '.realm // empty')

if [ "$REALM_EXISTS" = "systeminstaller" ]; then
    echo "⚠️  Realm 'systeminstaller' already exists"
    read -p "Do you want to delete and recreate it? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo "🗑️  Deleting existing realm..."
        curl -s -X DELETE \
          -H "Authorization: Bearer $ADMIN_TOKEN" \
          http://localhost:8082/admin/realms/systeminstaller
        echo "✅ Existing realm deleted"
    else
        echo "❌ Setup cancelled"
        exit 1
    fi
fi

# Import realm configuration
echo "📥 Importing SystemInstaller realm configuration..."
curl -s -X POST \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d @keycloak/systeminstaller-realm.json \
  http://localhost:8082/admin/realms

if [ $? -eq 0 ]; then
    echo "✅ Realm configuration imported successfully"
else
    echo "❌ Failed to import realm configuration"
    exit 1
fi

# Verify the configuration
echo "🔍 Verifying realm configuration..."
CLIENT_INFO=$(curl -s -H "Authorization: Bearer $ADMIN_TOKEN" \
  http://localhost:8082/admin/realms/systeminstaller/clients | \
  jq -r '.[] | select(.clientId=="systeminstaller-client") | {clientId, enabled, redirectUris}')

echo "✅ Client configuration:"
echo "$CLIENT_INFO" | jq .

echo ""
echo "🎉 Keycloak setup completed successfully!"
echo ""
echo "📋 Configuration Summary:"
echo "  Realm: systeminstaller"
echo "  Client ID: systeminstaller-client"
echo "  Client Secret: development-secret"
echo "  Gateway URL: http://localhost:5000"
echo "  Keycloak URL: http://localhost:8082"
echo ""
echo "🧪 Test Users:"
echo "  Admin: admin / admin123"
echo "  Customer: customer / customer123"
echo ""
echo "🚀 Next Steps:"
echo "  1. Start the Gateway: cd Gateway && dotnet run"
echo "  2. Open browser: http://localhost:5000/auth/login"
echo "  3. You should be redirected to Keycloak login"
echo ""
