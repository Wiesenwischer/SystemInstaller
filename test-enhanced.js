const { chromium } = require('playwright');

async function testRoutingAndLogout() {
  try {
    console.log('🚀 Testing routing and logout behavior...');
    
    const browser = await chromium.launch({ 
      headless: false,
      slowMo: 1000,
      args: ['--no-sandbox', '--disable-setuid-sandbox']
    });
    
    console.log('✅ Browser launched successfully!');
    
    const page = await browser.newPage();
    console.log('✅ New page created!');
    
    // Test 1: Check if health endpoint is working without auth
    console.log('\n📋 TEST 1: Health Endpoint (should work without auth)');
    try {
      const healthResponse = await page.goto('http://localhost:5000/health');
      console.log(`   Health Status: ${healthResponse.status()}`);
      if (healthResponse.status() === 200) {
        const healthData = await healthResponse.json();
        console.log(`   ✅ Health endpoint working: ${JSON.stringify(healthData)}`);
      } else {
        console.log(`   ❌ Health endpoint failed with status ${healthResponse.status()}`);
      }
    } catch (error) {
      console.log(`   ❌ Health endpoint error: ${error.message}`);
    }
    
    // Test 2: Check logout endpoint behavior
    console.log('\n📋 TEST 2: Logout Endpoint Behavior');
    try {
      const logoutResponse = await page.goto('http://localhost:5000/auth/logout');
      console.log(`   Logout Status: ${logoutResponse.status()}`);
      console.log(`   Final URL: ${page.url()}`);
      
      const pageTitle = await page.title();
      console.log(`   Page Title: ${pageTitle}`);
      
      if (page.url().includes('keycloak')) {
        console.log('   ✅ Correctly redirected to Keycloak for logout');
      } else if (pageTitle.includes('404') || pageTitle.includes('Not Found')) {
        console.log('   ❌ 404 ERROR DETECTED during logout!');
      } else {
        console.log('   ⚠️  Unexpected logout behavior');
      }
    } catch (error) {
      console.log(`   ❌ Logout endpoint error: ${error.message}`);
    }
    
    // Test 3: Full login -> logout cycle
    console.log('\n📋 TEST 3: Full Login -> Logout Cycle');
    console.log('🔐 Step 1: Accessing protected content (should redirect to login)');
    await page.goto('http://localhost:5000');
    
    try {
      await page.waitForURL('**/realms/systeminstaller/**', { timeout: 10000 });
      console.log('   ✅ Successfully redirected to Keycloak login');
      
      console.log('🔐 Step 2: Performing login');
      await page.fill('#username', 'admin');
      await page.fill('#password', 'admin123');
      await page.click('input[type="submit"]');
      
      await page.waitForURL('http://localhost:5000/', { timeout: 10000 });
      console.log('   ✅ Successfully logged in and redirected back');
      
      console.log('🚪 Step 3: Testing logout after authentication');
      await page.goto('http://localhost:5000/auth/logout');
      
      console.log('   ⏳ Waiting 3 seconds to see what happens...');
      await page.waitForTimeout(3000);
      
      const currentUrl = page.url();
      console.log(`   Final URL after logout: ${currentUrl}`);
      
      const title = await page.title();
      console.log(`   Page title: ${title}`);
      
      if (currentUrl.includes('keycloak')) {
        console.log('   ✅ Properly redirected to Keycloak for logout');
      } else {
        const bodyText = await page.locator('body').textContent();
        if (bodyText.includes('404') || title.includes('404')) {
          console.log('   ❌ 404 ERROR DETECTED! This is the problem you\'re experiencing.');
        } else {
          console.log('   ⚠️  Unexpected page content');
          console.log(`   Content preview: ${bodyText.substring(0, 200)}...`);
        }
      }
      
    } catch (error) {
      console.log(`   ❌ Authentication flow failed: ${error.message}`);
    }
    
    console.log('\n🎯 SUMMARY:');
    console.log('✅ Health endpoint should return JSON without requiring authentication');
    console.log('✅ Logout should redirect to Keycloak, not show 404 error');
    console.log('✅ The routing should be fixed now - check the results above!');
    
    console.log('\n👀 Look at the browser window to see the final state');
    await page.waitForTimeout(5000);
    
    await browser.close();
    console.log('✅ Test completed!');
    
  } catch (error) {
    console.error('❌ Error:', error.message);
    console.error('Full error:', error);
  }
}

testRoutingAndLogout();
