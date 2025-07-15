const { chromium } = require('playwright');

async function visualLogoutTest() {
  let browser;
  try {
    console.log('🚀 Visual Logout Test - You will see what happens in the browser');
    console.log('================================================');
    
    browser = await chromium.launch({ 
      headless: false,
      slowMo: 2000, // Slow down to see what's happening
      args: ['--no-sandbox', '--disable-setuid-sandbox']
    });
    
    const page = await browser.newPage();
    
    console.log('1. Navigating to SystemInstaller...');
    await page.goto('http://localhost:5000');
    await page.waitForTimeout(3000);
    
    console.log('2. Should be redirected to Keycloak login...');
    await page.waitForTimeout(2000);
    
    // Login
    console.log('3. Logging in with admin/admin123...');
    await page.fill('#username', 'admin');
    await page.fill('#password', 'admin123');
    await page.click('input[type="submit"]');
    
    console.log('4. Waiting for successful login...');
    await page.waitForTimeout(5000);
    
    console.log('5. Now initiating logout - WATCH THE BROWSER...');
    console.log('   You should see if a 404 error page appears');
    
    // Navigate to logout endpoint
    await page.goto('http://localhost:5000/auth/logout');
    
    console.log('6. Logout initiated - waiting 10 seconds...');
    console.log('   LOOK AT THE BROWSER: Do you see a 404 error page?');
    await page.waitForTimeout(10000);
    
    console.log('7. Current URL after logout:', page.url());
    console.log('8. Page title:', await page.title());
    
    // Check if we see 404 content
    const pageContent = await page.content();
    if (pageContent.includes('404') || pageContent.includes('Not Found') || pageContent.includes('Page not found')) {
      console.log('❌ 404 ERROR DETECTED in page content!');
      console.log('   This confirms the user is seeing a 404 error page');
    } else {
      console.log('✅ No 404 content detected in HTML');
    }
    
    console.log('\n🔍 VISUAL INSPECTION:');
    console.log('   - Look at the browser window now');
    console.log('   - Do you see a 404 error page?');
    console.log('   - What does the page actually show?');
    
    // Wait for manual inspection
    console.log('\n⏳ Waiting 15 seconds for you to inspect the browser...');
    await page.waitForTimeout(15000);
    
  } catch (error) {
    console.error('❌ Error during test:', error.message);
  } finally {
    if (browser) {
      console.log('🔚 Closing browser...');
      await browser.close();
    }
  }
}

visualLogoutTest();
