const { chromium } = require('playwright');

async function testFullLogoutFlow() {
  let browser;
  let page;
  
  try {
    console.log('🚀 Testing full login and logout flow...');
    
    browser = await chromium.launch({ 
      headless: false,
      slowMo: 1000,
      args: ['--no-sandbox', '--disable-setuid-sandbox']
    });
    
    page = await browser.newPage();
    
    // Enhanced request/response logging
    page.on('request', request => {
      console.log(`📤 REQUEST: ${request.method()} ${request.url()}`);
    });
    
    page.on('response', response => {
      console.log(`📥 RESPONSE: ${response.status()} ${response.url()}`);
    });
    
    // Step 1: Test login flow
    console.log('🔐 Testing login flow...');
    await page.goto('http://localhost:5000/api/protected');
    
    // Should redirect to Keycloak login
    await page.waitForURL('**/realms/systeminstaller/**', { timeout: 10000 });
    console.log('✅ Redirected to Keycloak login');
    
    // Fill login form
    await page.fill('#username', 'admin');
    await page.fill('#password', 'admin123');
    
    // Find and click login button
    const loginButton = await page.locator('input[type="submit"]').first();
    await loginButton.click();
    
    // Wait for successful login
    await page.waitForURL('http://localhost:5000/', { timeout: 10000 });
    console.log('✅ Successfully logged in');
    
    // Step 2: Test logout flow
    console.log('🚪 Testing logout flow...');
    
    // Navigate to logout endpoint
    await page.goto('http://localhost:5000/auth/logout');
    
    // Wait for page to load
    await page.waitForTimeout(3000);
    
    const currentUrl = page.url();
    console.log(`📍 Current URL after logout: ${currentUrl}`);
    
    // Check if we're on Keycloak
    if (currentUrl.includes('keycloak') || 
        currentUrl.includes('host.docker.internal:8082') ||
        currentUrl.includes('realms/systeminstaller')) {
      console.log('✅ LOGOUT SUCCESS: Redirected to Keycloak!');
      return true;
    } else {
      console.log('❌ LOGOUT FAILURE: Not redirected to Keycloak');
      console.log(`   Expected: URL containing 'keycloak' or 'host.docker.internal:8082'`);
      console.log(`   Actual: ${currentUrl}`);
      return false;
    }
    
  } catch (error) {
    console.error('❌ Test failed:', error.message);
    return false;
  } finally {
    if (page) {
      await page.waitForTimeout(2000);
    }
    if (browser) {
      await browser.close();
    }
  }
}

// Run the test
testFullLogoutFlow().then(success => {
  if (success) {
    console.log('\n🎉 FULL LOGOUT TEST PASSED! 🎉');
    process.exit(0);
  } else {
    console.log('\n💥 FULL LOGOUT TEST FAILED! 💥');
    process.exit(1);
  }
});
