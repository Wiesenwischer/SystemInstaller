#!/bin/bash

echo "🔧 Setting up Keycloak test user for SystemInstaller"
echo "=================================================="

# Keycloak admin credentials (from your docker-compose.yml)
KEYCLOAK_URL="http://localhost:8082"
ADMIN_USER="admin"
ADMIN_PASS="admin"
REALM="systeminstaller"

# Test user details
TEST_USER="testuser"
TEST_PASS="password"
TEST_EMAIL="testuser@systeminstaller.local"

echo "🔄 Step 1: Getting admin access token..."

# Get admin access token
TOKEN_RESPONSE=$(curl -s -X POST \
  "${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=admin-cli" \
  -d "username=${ADMIN_USER}" \
  -d "password=${ADMIN_PASS}")

if [ $? -ne 0 ]; then
  echo "❌ Failed to connect to Keycloak. Is it running on ${KEYCLOAK_URL}?"
  exit 1
fi

ACCESS_TOKEN=$(echo $TOKEN_RESPONSE | grep -o '"access_token":"[^"]*' | cut -d'"' -f4)

if [ -z "$ACCESS_TOKEN" ]; then
  echo "❌ Failed to get admin access token"
  echo "Response: $TOKEN_RESPONSE"
  exit 1
fi

echo "✅ Admin access token obtained"

echo "🔄 Step 2: Creating test user..."

# Create test user
USER_RESPONSE=$(curl -s -X POST \
  "${KEYCLOAK_URL}/admin/realms/${REALM}/users" \
  -H "Authorization: Bearer ${ACCESS_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "'${TEST_USER}'",
    "email": "'${TEST_EMAIL}'",
    "firstName": "Test",
    "lastName": "User",
    "enabled": true,
    "emailVerified": true,
    "credentials": [{
      "type": "password",
      "value": "'${TEST_PASS}'",
      "temporary": false
    }]
  }')

echo "🔄 Step 3: Checking if user was created..."

# Check if user exists
USER_CHECK=$(curl -s -X GET \
  "${KEYCLOAK_URL}/admin/realms/${REALM}/users?username=${TEST_USER}" \
  -H "Authorization: Bearer ${ACCESS_TOKEN}")

USER_COUNT=$(echo $USER_CHECK | grep -o '"username":"'${TEST_USER}'"' | wc -l)

if [ $USER_COUNT -gt 0 ]; then
  echo "✅ Test user '${TEST_USER}' created successfully"
  echo "📋 Test credentials:"
  echo "   Username: ${TEST_USER}"
  echo "   Password: ${TEST_PASS}"
  echo "   Email: ${TEST_EMAIL}"
else
  echo "⚠️  User might already exist or creation failed"
  echo "Response: $USER_CHECK"
fi

echo ""
echo "🧪 You can now run the logout tests with these credentials:"
echo "   Username: ${TEST_USER}"
echo "   Password: ${TEST_PASS}"
echo ""
echo "🔍 To verify, try logging in manually at: ${KEYCLOAK_URL}/realms/${REALM}/account"
