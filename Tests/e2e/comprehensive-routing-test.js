const { chromium } = require('playwright');

async function comprehensiveRoutingTest() {
  console.log('🚀 Starting Comprehensive YARP Routing and Logout Test');
  console.log('=' .repeat(80));
  
  const browser = await chromium.launch({ 
    headless: false,
    slowMo: 1000,
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });
  
  const page = await browser.newPage();
  
  try {
    // Test 1: Direct API/Auth Route Testing
    console.log('\n📋 TEST 1: Direct API/Auth Route Access');
    console.log('-'.repeat(50));
    
    const routes = [
      '/health',
      '/gateway/info', 
      '/auth/user',
      '/auth/login',
      '/auth/logout',
      '/api/protected'
    ];
    
    for (const route of routes) {
      try {
        console.log(`🔍 Testing route: ${route}`);
        const response = await page.goto(`http://localhost:5000${route}`, { 
          waitUntil: 'networkidle',
          timeout: 10000 
        });
        
        const status = response.status();
        const contentType = response.headers()['content-type'] || 'unknown';
        const url = response.url();
        
        console.log(`   Status: ${status}`);
        console.log(`   Content-Type: ${contentType}`);
        console.log(`   Final URL: ${url}`);
        
        // Check if this is a YARP proxied response (HTML from frontend)
        if (contentType.includes('text/html')) {
          const bodyText = await page.textContent('body');
          if (bodyText.includes('Keycloak') || bodyText.includes('<!DOCTYPE html>')) {
            console.log('   ⚠️  WARNING: Route appears to be proxied to frontend/Keycloak instead of handled by gateway API');
          }
        }
        
        console.log('');
      } catch (error) {
        console.log(`   ❌ Error: ${error.message}`);
        console.log('');
      }
    }
    
    // Test 2: Authentication Flow
    console.log('\n📋 TEST 2: Complete Authentication Flow');
    console.log('-'.repeat(50));
    
    console.log('🔐 Step 1: Accessing protected content (should redirect to login)');
    await page.goto('http://localhost:5000');
    
    try {
      await page.waitForURL('**/realms/systeminstaller/**', { timeout: 15000 });
      console.log('✅ Successfully redirected to Keycloak login');
      
      console.log('🔐 Step 2: Performing login');
      await page.fill('#username', 'admin');
      await page.fill('#password', 'admin123');
      await page.click('input[type="submit"]');
      
      await page.waitForURL('http://localhost:5000/', { timeout: 15000 });
      console.log('✅ Successfully logged in and redirected back');
      
    } catch (error) {
      console.log(`❌ Authentication flow failed: ${error.message}`);
      return;
    }
    
    // Test 3: Authenticated API Access
    console.log('\n📋 TEST 3: Authenticated API Access');
    console.log('-'.repeat(50));
    
    try {
      console.log('🔍 Testing /auth/user endpoint while authenticated');
      const userResponse = await page.goto('http://localhost:5000/auth/user');
      const userStatus = userResponse.status();
      const userContentType = userResponse.headers()['content-type'] || 'unknown';
      
      console.log(`   Status: ${userStatus}`);
      console.log(`   Content-Type: ${userContentType}`);
      
      if (userContentType.includes('application/json')) {
        const userData = await userResponse.json();
        console.log(`   ✅ JSON Response received: ${JSON.stringify(userData, null, 2)}`);
      } else {
        const bodyText = await page.textContent('body');
        console.log(`   ⚠️  Non-JSON response: ${bodyText.substring(0, 200)}...`);
      }
      
    } catch (error) {
      console.log(`   ❌ Error testing authenticated endpoint: ${error.message}`);
    }
    
    // Test 4: Logout Flow Analysis
    console.log('\n📋 TEST 4: Detailed Logout Flow Analysis');
    console.log('-'.repeat(50));
    
    console.log('🚪 Step 1: Initiating logout via /auth/logout');
    
    // Listen for all network requests during logout
    const logoutRequests = [];
    page.on('request', request => {
      logoutRequests.push({
        url: request.url(),
        method: request.method(),
        headers: request.headers()
      });
    });
    
    const logoutResponses = [];
    page.on('response', response => {
      logoutResponses.push({
        url: response.url(),
        status: response.status(),
        headers: response.headers(),
        contentType: response.headers()['content-type']
      });
    });
    
    try {
      const logoutResponse = await page.goto('http://localhost:5000/auth/logout', {
        waitUntil: 'networkidle',
        timeout: 20000
      });
      
      console.log(`   Initial logout response status: ${logoutResponse.status()}`);
      console.log(`   Final URL after logout: ${page.url()}`);
      
      const pageTitle = await page.title();
      console.log(`   Page title: ${pageTitle}`);
      
      const bodyText = await page.textContent('body');
      console.log(`   Page content preview: ${bodyText.substring(0, 300)}...`);
      
      // Check for 404 or error content
      if (bodyText.includes('404') || bodyText.includes('Not Found') || pageTitle.includes('404')) {
        console.log('   ❌ 404 ERROR DETECTED during logout!');
      } else if (bodyText.includes('Keycloak')) {
        console.log('   ✅ Properly redirected to Keycloak logout');
      } else {
        console.log('   ⚠️  Unexpected logout behavior');
      }
      
      console.log('\n🌐 Network Activity During Logout:');
      console.log('Requests:');
      logoutRequests.forEach((req, i) => {
        console.log(`   ${i + 1}. ${req.method} ${req.url}`);
      });
      
      console.log('Responses:');
      logoutResponses.forEach((res, i) => {
        console.log(`   ${i + 1}. ${res.status} ${res.url} (${res.contentType})`);
      });
      
    } catch (error) {
      console.log(`   ❌ Error during logout: ${error.message}`);
    }
    
    // Test 5: Post-Logout Session Validation
    console.log('\n📋 TEST 5: Post-Logout Session Validation');
    console.log('-'.repeat(50));
    
    console.log('🔍 Testing if session is truly cleared');
    try {
      const postLogoutUserResponse = await page.goto('http://localhost:5000/auth/user');
      const postLogoutStatus = postLogoutUserResponse.status();
      
      console.log(`   /auth/user status after logout: ${postLogoutStatus}`);
      
      if (postLogoutStatus === 401) {
        console.log('   ✅ Session properly cleared - returning 401 Unauthorized');
      } else if (postLogoutStatus === 200) {
        console.log('   ❌ Session NOT cleared - still returning user data!');
      } else {
        console.log(`   ⚠️  Unexpected status: ${postLogoutStatus}`);
      }
      
    } catch (error) {
      console.log(`   ❌ Error testing post-logout session: ${error.message}`);
    }
    
    // Test 6: F5 Refresh Problem Simulation
    console.log('\n📋 TEST 6: F5 Refresh Problem Simulation');
    console.log('-'.repeat(50));
    
    console.log('🔄 Simulating F5 refresh after logout');
    await page.reload({ waitUntil: 'networkidle' });
    
    const refreshUrl = page.url();
    console.log(`   URL after refresh: ${refreshUrl}`);
    
    if (refreshUrl.includes('localhost:5000') && !refreshUrl.includes('keycloak')) {
      console.log('   ❌ PROBLEM: Still on gateway URL - should redirect to Keycloak for re-auth');
    } else if (refreshUrl.includes('keycloak')) {
      console.log('   ✅ Properly redirected to Keycloak for re-authentication');
    }
    
    console.log('\n📊 TEST SUMMARY');
    console.log('=' .repeat(80));
    console.log('Check the browser window and the detailed output above to identify:');
    console.log('1. Are auth routes being handled by the gateway API or proxied by YARP?');
    console.log('2. Does logout properly redirect to Keycloak?');
    console.log('3. Is the session truly cleared after logout?');
    console.log('4. Does F5 refresh properly force re-authentication?');
    
    // Keep browser open for manual inspection
    console.log('\n⏳ Keeping browser open for 30 seconds for manual inspection...');
    await page.waitForTimeout(30000);
    
  } catch (error) {
    console.error('❌ Test failed:', error.message);
    console.error('Full error:', error);
  } finally {
    await browser.close();
    console.log('✅ Test completed - browser closed');
  }
}

// Run the test
comprehensiveRoutingTest().catch(console.error);
