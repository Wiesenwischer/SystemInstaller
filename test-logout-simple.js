const { chromium } = require('playwright');

async function testLogout() {
  console.log('🚀 Testing logout functionality...');
  
  const browser = await chromium.launch({ 
    headless: false,
    slowMo: 1000,
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });
  
  const page = await browser.newPage();
  
  try {
    // Step 1: Go to logout endpoint directly
    console.log('🚪 Testing logout endpoint...');
    await page.goto('http://localhost:5000/auth/logout');
    
    // Wait for redirect
    await page.waitForTimeout(5000);
    
    const currentUrl = page.url();
    console.log(`📍 Current URL after logout: ${currentUrl}`);
    
    // Check if we're redirected to Keycloak
    if (currentUrl.includes('keycloak') || currentUrl.includes('host.docker.internal:8082')) {
      console.log('✅ SUCCESS: Logout correctly redirected to Keycloak!');
      
      // Take screenshot
      await page.screenshot({ path: 'logout-success.png' });
      console.log('📸 Screenshot saved as logout-success.png');
      
      return true;
    } else {
      console.log('❌ FAILURE: Logout did not redirect to Keycloak');
      console.log(`   Expected: URL containing 'keycloak' or 'host.docker.internal:8082'`);
      console.log(`   Actual: ${currentUrl}`);
      
      // Take screenshot
      await page.screenshot({ path: 'logout-failure.png' });
      console.log('📸 Screenshot saved as logout-failure.png');
      
      return false;
    }
    
  } catch (error) {
    console.error('❌ Error during logout test:', error.message);
    return false;
  } finally {
    await page.waitForTimeout(3000);
    await browser.close();
  }
}

// Run the test
testLogout().then(success => {
  if (success) {
    console.log('\n🎉 LOGOUT TEST PASSED! 🎉');
    process.exit(0);
  } else {
    console.log('\n💥 LOGOUT TEST FAILED! 💥');
    process.exit(1);
  }
});
