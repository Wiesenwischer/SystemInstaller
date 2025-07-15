const { chromium } = require('playwright');

(async () => {
  console.log('🚀 Simple login test...');
  
  const browser = await chromium.launch({ headless: false });
  const page = await browser.newPage();
  
  try {
    // Track redirects
    let redirectCount = 0;
    page.on('response', response => {
      if (response.status() === 302) {
        redirectCount++;
        console.log(`📥 REDIRECT ${redirectCount}: ${response.url()}`);
        console.log(`📍 Location: ${response.headers().location}`);
      }
    });
    
    // Navigate to the root page
    console.log('📍 Navigating to localhost:5000...');
    await page.goto('http://localhost:5000', { waitUntil: 'domcontentloaded' });
    
    // Wait for the page to load
    await page.waitForTimeout(3000);
    
    const currentUrl = page.url();
    console.log(`📍 Current URL: ${currentUrl}`);
    
    // Check if we are redirected to Keycloak
    if (currentUrl.includes('host.docker.internal:8082')) {
      console.log('✅ Successfully redirected to Keycloak!');
      console.log('🔍 Looking for login form...');
      
      // Check if login form exists
      const usernameField = await page.$('#username');
      const passwordField = await page.$('#password');
      
      if (usernameField && passwordField) {
        console.log('✅ Login form found!');
        console.log('🎉 AUTHENTICATION FLOW WORKING CORRECTLY!');
      } else {
        console.log('❌ Login form not found');
      }
    } else {
      console.log('❌ NOT redirected to Keycloak');
      console.log(`📍 Current URL: ${currentUrl}`);
      
      if (redirectCount > 10) {
        console.log('❌ REDIRECT LOOP DETECTED!');
      }
    }
    
    console.log(`📊 Total redirects: ${redirectCount}`);
    
  } catch (error) {
    console.error('❌ Test failed:', error.message);
  } finally {
    await browser.close();
  }
})();
