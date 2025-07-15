#!/bin/bash

echo "🔍 Testing authentication flow step by step..."

echo ""
echo "1. Testing root path (/) - should redirect to /auth/login"
curl -v -L --max-redirs 1 "http://localhost:5000/" 2>&1 | grep -E "(HTTP/|Location:|< )"

echo ""
echo "2. Testing /auth/login - should redirect to Keycloak"  
curl -v -L --max-redirs 1 "http://localhost:5000/auth/login" 2>&1 | grep -E "(HTTP/|Location:|< )"

echo ""
echo "3. Testing if we can access Keycloak login page directly"
curl -s "http://localhost:8082/realms/systeminstaller/protocol/openid-connect/auth" | grep -q "Sign in" && echo "✅ Keycloak login page accessible" || echo "❌ Keycloak login page not accessible"

echo ""
echo "4. Testing gateway logs for authentication patterns..."
docker logs systeminstaller-gateway --tail=30 | grep -E "(auth|login|redirect|challenge)" || echo "No authentication logs found"
