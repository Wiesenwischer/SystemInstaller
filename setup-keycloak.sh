#!/bin/bash

# Keycloak Setup Script für SystemInstaller
# Dieses Script konfiguriert automatisch Keycloak für die Authentifizierung

echo "🚀 SystemInstaller Keycloak Setup"
echo "=================================="

# Warten bis Keycloak bereit ist
echo "⏳ Warte auf Keycloak..."
until curl -s http://localhost:8082/health/ready > /dev/null; do
    echo "Keycloak ist noch nicht bereit, warte 5 Sekunden..."
    sleep 5
done

echo "✅ Keycloak ist bereit!"

# Admin Token holen
echo "🔑 Hole Admin Token..."
ADMIN_TOKEN=$(curl -s -X POST \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin&password=admin123&grant_type=password&client_id=admin-cli" \
  http://localhost:8082/realms/master/protocol/openid-connect/token | \
  jq -r '.access_token')

if [ "$ADMIN_TOKEN" = "null" ] || [ -z "$ADMIN_TOKEN" ]; then
    echo "❌ Fehler beim Abrufen des Admin Tokens"
    exit 1
fi

echo "✅ Admin Token erhalten"

# Realm erstellen
echo "🏗️  Erstelle SystemInstaller Realm..."
curl -s -X POST \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "realm": "systeminstaller",
    "enabled": true,
    "displayName": "SystemInstaller",
    "loginTheme": "keycloak",
    "accessTokenLifespan": 3600,
    "ssoSessionIdleTimeout": 1800,
    "ssoSessionMaxLifespan": 36000
  }' \
  http://localhost:8082/admin/realms

echo "✅ Realm erstellt"

# Client erstellen
echo "🔧 Erstelle SystemInstaller Client..."
curl -s -X POST \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
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
      "https://localhost:5000/*"
    ],
    "postLogoutRedirectUris": [
      "http://localhost:5000/",
      "https://localhost:5000/"
    ],
    "webOrigins": [
      "http://localhost:5000",
      "https://localhost:5000"
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
  http://localhost:8082/admin/realms/systeminstaller/clients

echo "✅ Client erstellt"

# Client Secret holen
echo "🔐 Hole Client Secret..."
CLIENT_UUID=$(curl -s -H "Authorization: Bearer $ADMIN_TOKEN" \
  http://localhost:8082/admin/realms/systeminstaller/clients?clientId=systeminstaller-client | \
  jq -r '.[0].id')

CLIENT_SECRET=$(curl -s -H "Authorization: Bearer $ADMIN_TOKEN" \
  http://localhost:8082/admin/realms/systeminstaller/clients/$CLIENT_UUID/client-secret | \
  jq -r '.value')

echo "✅ Client Secret: $CLIENT_SECRET"

# Test User erstellen
echo "👤 Erstelle Test User..."
curl -s -X POST \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "enabled": true,
    "firstName": "Test",
    "lastName": "User",
    "email": "testuser@systeminstaller.local",
    "emailVerified": true,
    "credentials": [{
      "type": "password",
      "value": "password123",
      "temporary": false
    }]
  }' \
  http://localhost:8082/admin/realms/systeminstaller/users

echo "✅ Test User erstellt"

echo ""
echo "🎉 Setup erfolgreich abgeschlossen!"
echo ""
echo "📋 Konfigurationsdetails:"
echo "========================"
echo "Keycloak URL: http://localhost:8082"
echo "Realm: systeminstaller"
echo "Client ID: systeminstaller-client"
echo "Client Secret: $CLIENT_SECRET"
echo ""
echo "👤 Test Login:"
echo "Benutzername: testuser"
echo "Passwort: password123"
echo ""
echo "🔧 Aktualisiere bitte das Client Secret in den Konfigurationsdateien:"
echo "- appsettings.Development.json"
echo "- appsettings.json"
echo "- docker-compose.yml"
echo ""
echo "Ersetze 'your-client-secret' mit: $CLIENT_SECRET"
