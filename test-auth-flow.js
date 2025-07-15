const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({
    headless: false,
    slowMo: 1000
  });
  
  const page = await browser.newPage();
  
  try {
    console.log('🚀 Testing complete authentication flow...');
    
    // Navigate to the application
    console.log('📱 Navigating to http://localhost:5000');
    await page.goto('http://localhost:5000', { waitUntil: 'networkidle' });
    
    console.log('🔍 Current URL:', page.url());
    
    // Check if we're on Keycloak login page
    if (page.url().includes('host.docker.internal:8082') || page.url().includes('localhost:8082')) {
      console.log('✅ Successfully redirected to Keycloak login page!');
      
      // Wait for the login form to be visible
      await page.waitForSelector('#username', { timeout: 10000 });
      await page.waitForSelector('#password', { timeout: 10000 });
      await page.waitForSelector('#kc-login', { timeout: 10000 });
      
      console.log('� Login form detected, filling credentials...');
      
      // Fill in login credentials
      await page.fill('#username', 'admin');
      await page.fill('#password', 'admin123');
      
      console.log('🔑 Credentials filled, clicking login button...');
      
      // Click login button and wait for navigation
      await Promise.all([
        page.waitForNavigation({ timeout: 15000 }),
        page.click('#kc-login')
      ]);
      
      console.log('🔄 Login submitted, waiting for redirect...');
      console.log('🔍 Current URL after login:', page.url());
      
      // Check if we're redirected back to the application
      if (page.url().startsWith('http://localhost:5000')) {
        console.log('✅ Successfully authenticated and redirected back to application!');
        
        // Wait a bit to see if there's any redirect loop
        await page.waitForTimeout(3000);
        
        const finalUrl = page.url();
        console.log('🎯 Final URL:', finalUrl);
        
        if (finalUrl === 'http://localhost:5000/' || finalUrl.startsWith('http://localhost:5000/')) {
          console.log('🎉 Authentication flow completed successfully! No redirect loop detected.');
        } else {
          console.log('⚠️  Unexpected final URL. Possible redirect loop or other issue.');
        }
      } else {
        console.log('❌ Not redirected back to application. Current URL:', page.url());
      }
    } else {
      console.log('❌ Not redirected to Keycloak. Current URL:', page.url());
      
      // Check if we're already on the application (maybe already authenticated)
      if (page.url().startsWith('http://localhost:5000')) {
        console.log('ℹ️  Already on application page. Checking if user is authenticated...');
        
        // Try to access a protected endpoint to check authentication
        try {
          await page.goto('http://localhost:5000/auth/user', { waitUntil: 'networkidle' });
          console.log('✅ User appears to be authenticated');
        } catch (error) {
          console.log('❌ User not authenticated or other error:', error.message);
        }
      }
    }
    
    // Take a screenshot for verification
    await page.screenshot({ path: 'auth-flow-result.png' });
    console.log('📸 Screenshot saved as auth-flow-result.png');
    
  } catch (error) {
    console.error('❌ Error during authentication flow:', error.message);
    await page.screenshot({ path: 'auth-flow-error.png' });
    console.log('📸 Error screenshot saved as auth-flow-error.png');
  } finally {
    await browser.close();
  }
})();
