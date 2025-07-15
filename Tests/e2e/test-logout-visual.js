const { chromium } = require('playwright');

async function testLogout() {
  try {
    console.log('🚀 Testing logout to see if 404 error appears...');
    
    const browser = await chromium.launch({ 
      headless: false,
      slowMo: 2000,
      args: ['--no-sandbox', '--disable-setuid-sandbox']
    });
    
    console.log('✅ Browser launched successfully!');
    
    const page = await browser.newPage();
    console.log('✅ New page created!');
    
    console.log('🔗 Going directly to logout endpoint...');
    await page.goto('http://localhost:5000/auth/logout');
    
    console.log('⏳ Waiting 5 seconds to see what happens...');
    await page.waitForTimeout(5000);
    
    const currentUrl = page.url();
    console.log('📍 Current URL:', currentUrl);
    
    const title = await page.title();
    console.log('📝 Page title:', title);
    
    const bodyText = await page.locator('body').textContent();
    console.log('📄 Page content preview:', bodyText.substring(0, 200));
    
    if (bodyText.includes('404') || title.includes('404') || bodyText.includes('Not Found')) {
      console.log('❌ 404 ERROR DETECTED! This is what you\'re seeing.');
    } else {
      console.log('✅ No 404 error detected. The page loaded successfully.');
    }
    
    console.log('👀 Look at the browser - do you see a 404 error page?');
    await page.waitForTimeout(10000);
    
    await browser.close();
    console.log('✅ Browser closed - test completed!');
    
  } catch (error) {
    console.error('❌ Error:', error.message);
    console.error('Full error:', error);
  }
}

testLogout();
