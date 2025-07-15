const { chromium } = require('playwright');

(async () => {
  console.log('🚀 Testing login flow...');
  
  const browser = await chromium.launch({ headless: false });
  const page = await browser.newPage();
  
  try {
    // Navigate to the root page
    console.log('📍 Navigating to localhost:5000...');
    await page.goto('http://localhost:5000');
    
    // Wait for redirect to happen
    await page.waitForTimeout(2000);
    
    const currentUrl = page.url();
    console.log(`📍 Current URL: ${currentUrl}`);
    
    // Check if we are redirected to Keycloak
    if (currentUrl.includes('host.docker.internal:8082')) {
      console.log('✅ Successfully redirected to Keycloak!');
      
      // Try to login
      console.log('🔑 Attempting to login...');
      await page.fill('#username', 'admin');
      await page.fill('#password', 'admin123');
      await page.click('#kc-login');
      
      // Wait for redirect back to application
      await page.waitForTimeout(5000);
      
      const finalUrl = page.url();
      console.log(`📍 Final URL: ${finalUrl}`);
      
      if (finalUrl.includes('localhost:5000')) {
        console.log('✅ LOGIN SUCCESSFUL!');
        console.log('🎉 LOGIN FLOW TEST PASSED!');
      } else {
        console.log('❌ LOGIN FAILED - Still on Keycloak');
      }
    } else {
      console.log('❌ NOT redirected to Keycloak');
      console.log('🔍 Checking if we are in a redirect loop...');
      
      // Check for redirect loops by monitoring URL changes
      let urlChanges = 0;
      page.on('response', response => {
        if (response.status() === 302) {
          urlChanges++;
          console.log(`📥 REDIRECT ${urlChanges}: ${response.url()} → ${response.headers().location}`);
        }
      });
      
      await page.waitForTimeout(10000);
      
      if (urlChanges > 10) {
        console.log('❌ REDIRECT LOOP DETECTED!');
      }
    }
    
  } catch (error) {
    console.error('❌ Test failed:', error.message);
  } finally {
    await browser.close();
  }
})();
