#!/bin/bash

echo "🚀 Setting up SystemInstaller E2E Tests"

# Navigate to test directory
cd "$(dirname "$0")"

# Install dependencies
echo "📦 Installing dependencies..."
npm install

# Install Playwright browsers
echo "🌐 Installing Playwright browsers..."
npx playwright install

# Create test credentials file
cat > .env.test << EOF
# Test configuration for SystemInstaller E2E tests
TEST_BASE_URL=http://localhost:5000
KEYCLOAK_URL=http://localhost:8082
TEST_USERNAME=testuser
TEST_PASSWORD=password
EOF

echo "✅ Setup complete!"
echo ""
echo "📋 Available commands:"
echo "  npm test              - Run all tests headless"
echo "  npm run test:headed   - Run tests with browser UI"
echo "  npm run test:debug    - Run tests in debug mode"
echo ""
echo "⚠️  Make sure SystemInstaller is running on localhost:5000 before running tests"
echo "⚠️  Make sure Keycloak is running on localhost:8082 before running tests"
