const { chromium } = require('playwright');

async function quickLogoutTest() {
  let browser;
  let page;
  
  try {
    console.log('🚀 Quick logout test starting...');
    
    browser = await chromium.launch({ 
      headless: true,  // Faster execution
      slowMo: 0        // No delay
    });
    
    page = await browser.newPage();
    
    // Enhanced request/response logging
    page.on('response', response => {
      console.log(`📥 RESPONSE: ${response.status()} ${response.url()}`);
      if (response.status() >= 400) {
        console.log(`❌ ERROR RESPONSE: ${response.status()} - ${response.statusText()}`);
      }
    });
    
    console.log('🔍 Testing direct logout endpoint access...');
    await page.goto('http://localhost:5000/auth/logout');
    
    // Wait a moment for any redirects
    await page.waitForTimeout(2000);
    
    const currentUrl = page.url();
    const title = await page.title();
    
    console.log('\n📊 LOGOUT ENDPOINT TEST RESULTS:');
    console.log('=================================');
    console.log(`📍 URL after accessing logout: ${currentUrl}`);
    console.log(`📝 Page title: ${title}`);
    
    // Get page content to check for 404
    let bodyText = '';
    try {
      bodyText = await page.locator('body').textContent();
      console.log(`📄 Body content: ${bodyText.substring(0, 200)}`);
    } catch (error) {
      console.log(`⚠️  Could not read body content: ${error.message}`);
    }
    
    // Analysis
    if (bodyText.includes('404') || title.includes('404') || title.includes('Not Found')) {
      console.log('\n❌ 404 ERROR DETECTED!');
      console.log('❌ The logout endpoint is returning a 404 error.');
      console.log('❌ This means YARP routing is not working correctly.');
      return false;
    } else if (currentUrl.includes('keycloak')) {
      console.log('\n✅ GOOD: Logout redirected to Keycloak!');
      console.log('✅ This means the logout endpoint is working correctly.');
      return true;
    } else if (currentUrl === 'http://localhost:5000/auth/logout') {
      console.log('\n⚠️  Stayed on logout URL - checking response...');
      if (bodyText.trim() === '' || bodyText.includes('Logged out')) {
        console.log('✅ Empty or success response - logout might be working');
        return true;
      } else {
        console.log('❌ Unexpected content on logout page');
        return false;
      }
    } else {
      console.log(`\n⚠️  Unexpected redirect to: ${currentUrl}`);
      return false;
    }
    
  } catch (error) {
    console.error('\n❌ Quick test failed:', error.message);
    return false;
  } finally {
    if (browser) {
      await browser.close();
    }
  }
}

// Run the quick test
console.log('Starting quick logout endpoint test...');
quickLogoutTest().then(success => {
  if (success) {
    console.log('\n🎉 LOGOUT ENDPOINT TEST PASSED! 🎉');
    console.log('The logout functionality appears to be working correctly.');
  } else {
    console.log('\n💥 LOGOUT ENDPOINT TEST FAILED! 💥');
    console.log('There is an issue with the logout endpoint routing.');
  }
});
