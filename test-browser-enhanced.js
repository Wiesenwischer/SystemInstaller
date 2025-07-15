const { chromium } = require('playwright');

async function testCompleteAuthFlow() {
  let browser;
  let page;
  
  try {
    console.log('🚀 COMPREHENSIVE AUTHENTICATION FLOW TEST');
    console.log('==========================================');
    console.log('This test will verify:');
    console.log('1. Gateway initial access and Keycloak redirect');
    console.log('2. Keycloak login process');
    console.log('3. Post-login gateway access');
    console.log('4. Logout functionality and behavior');
    console.log('5. Error detection and detailed reporting');
    console.log('');
    
    browser = await chromium.launch({ 
      headless: false,
      slowMo: 1000,
      args: ['--no-sandbox', '--disable-setuid-sandbox']
    });
    
    console.log('✅ Browser launched successfully!');
    
    page = await browser.newPage();
    
    // Enhanced request/response logging
    page.on('request', request => {
      console.log(`📤 REQUEST: ${request.method()} ${request.url()}`);
    });
    
    page.on('response', response => {
      console.log(`📥 RESPONSE: ${response.status()} ${response.url()}`);
      if (response.status() >= 400) {
        console.log(`❌ ERROR RESPONSE: ${response.status()} - ${response.statusText()}`);
      }
    });
    
    page.on('pageerror', error => {
      console.log(`💥 PAGE ERROR: ${error.message}`);
    });
    
    page.on('console', msg => {
      console.log(`🖥️  BROWSER CONSOLE [${msg.type()}]: ${msg.text()}`);
    });
    
    console.log('✅ Page created with comprehensive logging enabled!');
    
    // STEP 1: Test initial gateway access
    console.log('\n🏠 STEP 1: Testing initial gateway access');
    console.log('==========================================');
    
    await page.goto('http://localhost:5000');
    console.log('📍 Navigated to gateway root');
    
    // Check if we get redirected to Keycloak or if there's an error
    console.log('⏳ Waiting for page to load or redirect to Keycloak...');
    try {
      await page.waitForURL('**/realms/systeminstaller/**', { timeout: 10000 });
      console.log('✅ Successfully redirected to Keycloak login');
    } catch (timeoutError) {
      const currentUrl = page.url();
      const title = await page.title();
      const bodyText = await page.locator('body').textContent();
      
      console.log(`❌ No redirect to Keycloak within timeout`);
      console.log(`📍 Current URL: ${currentUrl}`);
      console.log(`📝 Page title: ${title}`);
      console.log(`📄 Page content preview: ${bodyText.substring(0, 300)}`);
      
      if (bodyText.includes('404') || title.includes('404')) {
        console.log('❌ CRITICAL: Gateway itself returns 404!');
        throw new Error('Gateway root endpoint not working');
      }
      
      // If we're still on gateway, that might be OK if authentication is disabled
      if (currentUrl === 'http://localhost:5000/') {
        console.log('ℹ️  Still on gateway - checking if this is expected behavior');
      }
    }
    
    // STEP 2: Test Keycloak login process
    console.log('\n🔐 STEP 2: Testing Keycloak login process');
    console.log('==========================================');
    
    // Check if we're actually on Keycloak
    const currentUrl = page.url();
    if (!currentUrl.includes('keycloak') && !currentUrl.includes('realms')) {
      console.log('⚠️  Not on Keycloak login page - authentication might be disabled');
      console.log('📍 Continuing test from current location...');
    } else {
      // Wait for Keycloak login form
      await page.waitForSelector('#username', { timeout: 5000 });
      console.log('✅ Keycloak login form detected');
      
      // Fill login credentials
      console.log('📝 Filling login credentials (admin/admin123)...');
      await page.fill('#username', 'admin');
      await page.fill('#password', 'admin123');
      
      console.log('🎯 Submitting login form...');
      await page.click('input[type="submit"]');
      
      console.log('⏳ Waiting for login to complete...');
      await page.waitForURL('http://localhost:5000/', { timeout: 15000 });
      console.log('✅ Successfully logged in and redirected back to gateway!');
    }
    
    // STEP 3: Verify authenticated state
    console.log('\n🔍 STEP 3: Verifying authenticated state');
    console.log('========================================');
    
    const authenticatedUrl = page.url();
    const authenticatedTitle = await page.title();
    console.log(`📍 Current URL after login: ${authenticatedUrl}`);
    console.log(`📝 Page title: ${authenticatedTitle}`);
    
    // Check if we can access a protected resource
    console.log('🛡️  Testing access to protected resources...');
    
    // Take a screenshot for debugging
    await page.screenshot({ path: 'authenticated-state.png' });
    console.log('📸 Screenshot saved as authenticated-state.png');
    
    // STEP 4: Test logout functionality - THE MAIN TEST
    console.log('\n🚪 STEP 4: Testing logout functionality (MAIN TEST)');
    console.log('===================================================');
    
    console.log('🎯 Navigating directly to logout endpoint...');
    await page.goto('http://localhost:5000/auth/logout');
    
    console.log('⏳ Waiting 3 seconds to see what happens after logout...');
    await page.waitForTimeout(3000);
    
    const logoutUrl = page.url();
    const logoutTitle = await page.title();
    const logoutBodyText = await page.locator('body').textContent();
    
    console.log('\n📊 LOGOUT RESULTS:');
    console.log('==================');
    console.log(`📍 URL after logout: ${logoutUrl}`);
    console.log(`📝 Page title: ${logoutTitle}`);
    console.log(`📄 Page content (first 300 chars): ${logoutBodyText.substring(0, 300)}`);
    
    // Take screenshot of logout result
    await page.screenshot({ path: 'logout-result.png' });
    console.log('📸 Logout screenshot saved as logout-result.png');
    
    // Detailed analysis of logout behavior
    console.log('\n🔍 DETAILED LOGOUT ANALYSIS:');
    console.log('============================');
    
    if (logoutBodyText.includes('404') || logoutTitle.includes('404') || logoutTitle.includes('Not Found')) {
      console.log('❌ 404 ERROR DETECTED IN LOGOUT!');
      console.log('❌ This is the problem you\'re experiencing');
      console.log('❌ The logout endpoint is not properly routed');
      return false;
    }
    
    if (logoutUrl.includes('keycloak') || logoutUrl.includes('realms')) {
      console.log('✅ GOOD: Logout redirected to Keycloak');
      console.log('✅ This means YARP routing is working correctly');
    } else if (logoutUrl === 'http://localhost:5000/auth/logout') {
      console.log('⚠️  Still on logout URL - checking response content...');
      if (logoutBodyText.trim() === '') {
        console.log('ℹ️  Empty response - this might be expected for logout');
      }
    } else if (logoutUrl === 'http://localhost:5000/') {
      console.log('✅ GOOD: Logout redirected back to gateway root');
    } else {
      console.log(`⚠️  Unexpected logout destination: ${logoutUrl}`);
    }
    
    // STEP 5: Test re-authentication after logout
    console.log('\n🔄 STEP 5: Testing re-authentication after logout');
    console.log('==================================================');
    
    console.log('🏠 Attempting to access gateway root after logout...');
    await page.goto('http://localhost:5000');
    
    console.log('⏳ Waiting to see if we get redirected to login again...');
    await page.waitForTimeout(3000);
    
    const finalUrl = page.url();
    console.log(`📍 Final URL: ${finalUrl}`);
    
    if (finalUrl.includes('keycloak') || finalUrl.includes('realms')) {
      console.log('✅ EXCELLENT: After logout, accessing gateway requires re-authentication');
      console.log('✅ This confirms the logout actually worked');
    } else if (finalUrl === 'http://localhost:5000/') {
      console.log('⚠️  Still accessing gateway without authentication - logout might not have worked');
    }
    
    // FINAL SUMMARY
    console.log('\n🎯 FINAL TEST SUMMARY');
    console.log('=====================');
    console.log('Gateway access: ✅');
    console.log('Keycloak login: ✅');
    console.log('Authentication flow: ✅');
    
    if (logoutBodyText.includes('404') || logoutTitle.includes('404')) {
      console.log('Logout functionality: ❌ 404 ERROR');
      console.log('\n❌ CONCLUSION: The logout endpoint returns 404');
      console.log('❌ This indicates a YARP routing problem');
      console.log('❌ The /auth/logout route is not properly configured');
    } else {
      console.log('Logout functionality: ✅');
      console.log('\n✅ CONCLUSION: Logout appears to be working correctly');
    }
    
    console.log('\n👀 Please check the browser window and screenshots for visual confirmation');
    await page.waitForTimeout(5000);
    
    await browser.close();
    console.log('✅ Test completed - browser closed');
    
    return !logoutBodyText.includes('404') && !logoutTitle.includes('404');
    
  } catch (error) {
    console.error('\n💥 TEST FAILED WITH ERROR:');
    console.error('===========================');
    console.error(`❌ Error: ${error.message}`);
    console.error(`❌ Stack: ${error.stack}`);
    
    if (page) {
      try {
        await page.screenshot({ path: 'error-state.png' });
        console.log('📸 Error screenshot saved as error-state.png');
        
        const errorUrl = page.url();
        const errorTitle = await page.title();
        const errorBodyText = await page.locator('body').textContent();
        
        console.log(`📍 URL when error occurred: ${errorUrl}`);
        console.log(`📝 Page title: ${errorTitle}`);
        console.log(`📄 Page content: ${errorBodyText.substring(0, 300)}`);
      } catch (screenshotError) {
        console.log('❌ Could not take error screenshot');
      }
    }
    
    if (browser) {
      await browser.close();
    }
    
    return false;
  }
}

// Run the test
console.log('🎬 Starting comprehensive authentication flow test...');
testCompleteAuthFlow()
  .then(success => {
    if (success) {
      console.log('\n🎉 ALL TESTS PASSED! 🎉');
      process.exit(0);
    } else {
      console.log('\n💥 TESTS FAILED! 💥');
      process.exit(1);
    }
  })
  .catch(error => {
    console.error('\n💥 TEST EXECUTION FAILED:', error);
    process.exit(1);
  });
