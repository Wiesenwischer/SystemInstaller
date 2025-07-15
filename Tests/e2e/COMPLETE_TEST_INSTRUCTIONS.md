# 🧪 SystemInstaller Logout Test - Complete Instructions

## 📋 Prerequisites Setup

### 1. Start SystemInstaller
```bash
cd d:\proj\SystemInstaller
docker-compose up -d
```

Wait for all containers to be running (especially Keycloak which takes time to start).

### 2. Create Test User in Keycloak

**Option A: Automatic (if curl works):**
```bash
cd d:\proj\SystemInstaller\tests\e2e
setup-test-user.sh  # Linux/Mac
setup-test-user.bat # Windows
```

**Option B: Manual (Recommended):**
1. Open browser to: http://localhost:8082/admin/
2. Login with:
   - Username: `admin`
   - Password: `admin`
3. Select "systeminstaller" realm from dropdown
4. Go to "Users" → "Create new user"
5. Fill form:
   - Username: `testuser`
   - Email: `testuser@test.com`
   - First name: `Test`
   - Last name: `User`
   - Email verified: ✅ ON
   - Enabled: ✅ ON
6. Click "Create"
7. Go to "Credentials" tab → "Set password"
8. Set password: `password`
9. Temporary: ❌ OFF
10. Click "Save"

### 3. Verify Test User
Test login at: http://localhost:8082/realms/systeminstaller/account
- Username: `testuser`
- Password: `password`

## 🚀 Running the Logout Test

### Option 1: Automated Test (Playwright)
```bash
cd d:\proj\SystemInstaller\tests\e2e
npm install
npx playwright install
npm run test:headed -- flexible-logout.spec.ts
```

### Option 2: Manual Test (Recommended for first run)

**Step-by-Step Manual Test:**

1. **Open browser to http://localhost:5000**
2. **Login with testuser/password**
3. **Verify you see the dashboard/homepage**
4. **Click the logout button** (or go to http://localhost:5000/auth/logout)
5. **Wait for logout to complete**
6. **🔴 CRITICAL: Hit F5 (refresh the page)**
7. **Check what you see:**
   - ✅ **CORRECT:** Login page (Keycloak asks for credentials)
   - ❌ **BUG:** Dashboard/homepage (automatically logged back in)

## 🎯 Expected Results (Current Implementation)

### What SHOULD happen (correct logout):
1. Login → Dashboard ✅
2. Logout → Keycloak logout page ✅  
3. F5 refresh → Login page (must enter credentials) ✅

### What ACTUALLY happens (current bug):
1. Login → Dashboard ✅
2. Logout → "Welcome to SystemInstaller" page ✅
3. F5 refresh → **Dashboard again** ❌ (AUTO-LOGIN BUG)

## 🔍 Debug Information

### Check Gateway Logs:
```bash
docker logs systeminstaller-gateway --tail 50
```

Look for logout debug messages:
- "=== LOGOUT ENDPOINT CALLED ==="
- "User authenticated: true"
- "Local sign out completed"
- "Redirecting to Keycloak logout"

### Check Authentication Status:
```bash
# Should return 401 after proper logout
curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/auth/user
```

## 🐛 The Problem

The issue is **Keycloak session persistence**:

1. Gateway logout clears local session ✅
2. But Keycloak session remains active ❌
3. F5 refresh → Gateway redirects to Keycloak → Silent re-authentication ❌

## 🛠️ Fix Required

The logout endpoint needs to properly terminate the Keycloak session by ensuring:
1. `id_token_hint` is included in logout URL
2. Proper Keycloak logout URL formation
3. Session cleanup on both sides

## 📊 Success Criteria

The logout is working correctly when:
- ✅ After logout + F5: User sees login page
- ✅ `/auth/user` returns 401 (not 200)
- ✅ User must re-enter credentials
- ✅ No automatic silent re-authentication

---

**🎯 Run the manual test above to confirm the current bug, then we can implement the fix!**
