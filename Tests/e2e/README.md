# ReadyStackGo (RSGO) E2E Tests

<div align="center">
  <img src="../../assets/logo.png" alt="ReadyStackGo Logo" width="200">
</div>

> **Turn your specs into stacks**

This directory contains comprehensive end-to-end tests for the ReadyStackGo logout functionality.

## 🎯 Purpose

These tests are designed to **FAIL** with the current implementation to clearly demonstrate the logout issue where:
- User logs out successfully
- User hits F5 (refresh)
- User is automatically logged back in (❌ THIS IS THE BUG)

## 🔧 Setup

### Windows:
```bash
cd d:\proj\SystemInstaller\tests\e2e
setup.bat
```

### Linux/Mac:
```bash
cd /path/to/SystemInstaller/tests/e2e
chmod +x setup.sh
./setup.sh
```

## 🏃‍♂️ Running Tests

**Prerequisites:** Make sure your SystemInstaller application is running:
```bash
# In your main project directory
docker-compose up -d
```

### Run Tests:
```bash
# Run all tests (headless)
npm test

# Run with browser UI visible
npm run test:headed

# Run in debug mode (step through)
npm run test:debug
```

## 📋 Test Scenarios

### 1. Critical Logout Flow Test
**What it tests:**
- User logs in successfully
- User clicks logout
- User hits F5 (refresh)
- **EXPECTATION:** User should see login page
- **CURRENT BEHAVIOR:** User sees dashboard (❌ BUG)

### 2. Multi-tab Session Test
**What it tests:**
- Logout in one tab terminates session in all tabs

### 3. API Access After Logout
**What it tests:**
- Protected endpoints return 401 after logout

### 4. Gateway Debug Information
**What it tests:**
- Logout endpoint debug logs are generated

## 📊 Expected Results (Current State)

With the current implementation, the main test **SHOULD FAIL** with something like:

```
❌ expect(postRefreshAuth).toBe(401);
   Expected: 401
   Received: 200

   User is automatically re-authenticated after F5 refresh
```

## 🐛 What This Proves

When the test fails, it proves:
1. The logout button works (local session cleared)
2. The Keycloak session is NOT properly terminated
3. F5 refresh triggers automatic re-authentication via Keycloak
4. The logout flow is incomplete

## 🔍 Debugging Information

The tests will generate:
- Screenshots at each step (`test-results/` folder)
- Console logs showing the exact flow
- HTTP response codes for API calls
- Current URLs at each step

## 🎯 Success Criteria

The tests will PASS when:
1. After logout + F5, user sees login page (not dashboard)
2. API calls return 401 (not 200) after logout
3. User must re-enter credentials to access the application
4. Keycloak session is properly terminated

## 🔧 Fixing the Issue

The tests point to these areas that need fixing:
1. **Keycloak logout URL** - Must include proper parameters
2. **Session termination** - Both local and Keycloak sessions
3. **Redirect handling** - Post-logout redirect flow
4. **Token cleanup** - Proper cleanup of all authentication tokens

Run these tests to see the exact failure points and measure progress as you fix the logout implementation.
