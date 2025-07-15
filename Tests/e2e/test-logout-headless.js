const { chromium } = require('playwright');

async function testLogout() {
  try {
    console.log('🚀 Testing logout to see if 404 error appears...');
    
    const browser = await chromium.launch({ 
      headless: true,
      args: ['--no-sandbox', '--disable-setuid-sandbox']
    });
    
    console.log('✅ Browser launched successfully!');
    
    const page = await browser.newPage();
    console.log('✅ New page created!');
    
    console.log('🔗 Going directly to logout endpoint...');
    
    try {
      await page.goto('http://localhost:5000/auth/logout', {
        waitUntil: 'networkidle',
        timeout: 10000
      });
    } catch (error) {
      console.log('❌ Navigation error:', error.message);
    }
    
    console.log('⏳ Checking current state...');
    
    const currentUrl = page.url();
    console.log('📍 Current URL:', currentUrl);
    
    const title = await page.title();
    console.log('📝 Page title:', title);
    
    const bodyText = await page.locator('body').textContent();
    console.log('📄 Page content preview:', bodyText.substring(0, 300));
    
    if (bodyText.includes('404') || title.includes('404') || bodyText.includes('Not Found')) {
      console.log('🔴 404 ERROR DETECTED! This is what you\'re seeing.');
    } else {
      console.log('✅ No 404 error detected. The page loaded successfully.');
    }
    
    // Check for error status
    const response = await page.goto('http://localhost:5000/auth/logout');
    if (response) {
      console.log('📊 Response status:', response.status());
      console.log('📊 Response headers:', JSON.stringify(await response.allHeaders(), null, 2));
    }
    
    await browser.close();
    console.log('✅ Browser closed - test completed!');
    
  } catch (error) {
    console.error('❌ Error:', error.message);
    console.error('Full error:', error);
  }
}

testLogout();
